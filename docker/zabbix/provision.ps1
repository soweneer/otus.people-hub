param(
    [string]$BaseUrl = "http://localhost:8095/api_jsonrpc.php",
    [string]$UserName = "Admin",
    [string]$Password = "zabbix",
    [string]$TargetHost = "chats-host",
    [string]$AgentDns = "zabbix-agent",
    [string]$GroupName = "People Hub"
)

$ErrorActionPreference = "Stop"
$script:Token = $null

function Invoke-Zabbix {
    param([string]$Method, $Params)

    $headers = @{ "Content-Type" = "application/json-rpc" }
    if ($script:Token) {
        $headers["Authorization"] = "Bearer $script:Token"
    }

    $body = @{ jsonrpc = "2.0"; method = $Method; params = $Params; id = 1 } | ConvertTo-Json -Depth 10
    $response = Invoke-RestMethod -Uri $BaseUrl -Method Post -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($body))

    if ($response.PSObject.Properties.Name -contains "error") {
        throw "$Method -> $($response.error.message) $($response.error.data)"
    }

    return $response.result
}

function Wait-ZabbixApi {
    $probe = '{"jsonrpc":"2.0","method":"apiinfo.version","params":{},"id":1}'

    for ($attempt = 1; $attempt -le 60; $attempt++) {
        try {
            $version = (Invoke-RestMethod -Uri $BaseUrl -Method Post -Headers @{ "Content-Type" = "application/json-rpc" } -Body $probe).result
            Write-Host "Zabbix API $version доступен"

            return
        }
        catch {
            Start-Sleep -Seconds 5
        }
    }

    throw "Zabbix API не отвечает по адресу $BaseUrl"
}

function Resolve-GroupId {
    $existing = Invoke-Zabbix -Method "hostgroup.get" -Params @{ output = @("groupid"); filter = @{ name = @($GroupName) } }
    if ($existing.Count -gt 0) {
        return $existing[0].groupid
    }

    return (Invoke-Zabbix -Method "hostgroup.create" -Params @{ name = $GroupName }).groupids[0]
}

function Resolve-TemplateRefs {
    param([string[]]$Names)

    $found = Invoke-Zabbix -Method "template.get" -Params @{ output = @("templateid", "host"); filter = @{ host = $Names } }

    foreach ($name in $Names) {
        if (-not ($found | Where-Object { $_.host -eq $name })) {
            throw "Шаблон '$name' не найден в этой инсталляции Zabbix"
        }
    }

    return @($found | ForEach-Object { @{ templateid = $_.templateid } })
}

function Resolve-HostId {
    param($GroupId, $TemplateRefs)

    $existing = Invoke-Zabbix -Method "host.get" -Params @{ output = @("hostid"); filter = @{ host = @($TargetHost) } }
    if ($existing.Count -gt 0) {
        Invoke-Zabbix -Method "host.update" -Params @{ hostid = $existing[0].hostid; templates = $TemplateRefs } | Out-Null
        Write-Host "Хост $TargetHost уже был, шаблоны переприкреплены"

        return $existing[0].hostid
    }

    $created = Invoke-Zabbix -Method "host.create" -Params @{
        host       = $TargetHost
        groups     = @(@{ groupid = $GroupId })
        templates  = $TemplateRefs
        interfaces = @(@{ type = 1; main = 1; useip = 0; ip = ""; dns = $AgentDns; port = "10050" })
    }
    Write-Host "Хост $TargetHost создан"

    return $created.hostids[0]
}

function Add-Triggers {
    param($HostId)

    $definitions = @(
        @{
            ItemKey     = "system.cpu.util"
            Description = "Сервис чатов: загрузка CPU выше 80% пять минут"
            Expression  = "min(/$TargetHost/system.cpu.util,5m)>80"
        },
        @{
            ItemKey     = "vm.memory.size[pavailable]"
            Description = "Сервис чатов: свободной памяти меньше 20%"
            Expression  = "max(/$TargetHost/vm.memory.size[pavailable],5m)<20"
        },
        @{
            ItemKey     = "vm.memory.utilization"
            Description = "Сервис чатов: память занята больше чем на 80%"
            Expression  = "min(/$TargetHost/vm.memory.utilization,5m)>80"
        }
    )

    $itemKeys = @(Invoke-Zabbix -Method "item.get" -Params @{ output = @("key_"); hostids = $HostId } | ForEach-Object { $_.key_ })

    foreach ($definition in $definitions) {
        if ($itemKeys -notcontains $definition.ItemKey) {
            Write-Host "Пропущен триггер '$($definition.Description)': на хосте нет элемента $($definition.ItemKey)"
            continue
        }

        $existing = Invoke-Zabbix -Method "trigger.get" -Params @{
            output  = @("triggerid")
            hostids = $HostId
            filter  = @{ description = @($definition.Description) }
        }
        if ($existing.Count -gt 0) {
            continue
        }

        Invoke-Zabbix -Method "trigger.create" -Params @{
            description = $definition.Description
            expression  = $definition.Expression
            priority    = 4
        } | Out-Null
        Write-Host "Создан триггер '$($definition.Description)'"
    }
}

Wait-ZabbixApi

$script:Token = Invoke-Zabbix -Method "user.login" -Params @{ username = $UserName; password = $Password }

$groupId = Resolve-GroupId
$templateRefs = Resolve-TemplateRefs -Names @("Linux by Zabbix agent", "Docker by Zabbix agent 2")
$hostId = Resolve-HostId -GroupId $groupId -TemplateRefs $templateRefs

Add-Triggers -HostId $hostId

Write-Host "Готово: http://localhost:8095 -> Monitoring -> Latest data -> $TargetHost"
