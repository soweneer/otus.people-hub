using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Exceptions;
using PeopleHub.Extensions;
using PeopleHub.Domain.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class PersonController(IPersonService personService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var persons = await personService.GetFriendsAsync(User.Identity!.Name, HttpContext.RequestAborted);

            return View(persons);
        }

        [HttpGet]
        public async Task<IActionResult> Person(int personId)
        {
            var personInfo = await personService.GetByIdAsync(User.Identity!.Name, personId, HttpContext.RequestAborted);

            return personInfo == null 
                ? throw new UnknownPersonException(personId)
                : View(personInfo);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var personInfo = await personService.GetByIdAsync(User.Identity!.Name, null, HttpContext.RequestAborted);

            return View(personInfo);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdateMyProfileRequest updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var updatedPerson = await personService.UpdateAsync(User.Identity!.Name, updateRequest.ExtractPersonalInfo(), HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Профиль успешно сохранен";

            return View(updatedPerson);
        }
    }
}
