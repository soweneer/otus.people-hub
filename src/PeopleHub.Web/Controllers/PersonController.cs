using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Exceptions;
using PeopleHub.Domain.Model.Dto.Person;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class PersonController(IMapper mapper, IPersonRepository personRepository) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity is null)
            {
                return Unauthorized();
            }

            var persons = await personRepository.GetAllWithFriendStatusAsync(
                User.Identity.Name, HttpContext.RequestAborted);

            return View(persons);
        }

        [HttpGet]
        public async Task<IActionResult> Person(int personId)
        {
            if (User.Identity is null) return Unauthorized();

            var curPersonId = await personRepository.GetPersonIdAsync(User.Identity.Name,
                HttpContext.RequestAborted);
            var personInfo = await personRepository.GetByIdAsync(personId, curPersonId, HttpContext.RequestAborted);
            if (personInfo == null)
                throw new UnknownPersonException(personId);

            return View(personInfo);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity is null) return Unauthorized();
            var curPersonId = await personRepository.GetPersonIdAsync(User.Identity.Name,
                HttpContext.RequestAborted);

            var personInfo = await personRepository.GetByIdAsync(curPersonId, null, HttpContext.RequestAborted);
            return View(mapper.Map<UpdatePersonDto>(personInfo));
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdatePersonDto profileData)
        {
            if (User.Identity is null) return Unauthorized();
            if (!ModelState.IsValid)
                return View();
            var curPersonId = await personRepository.GetPersonIdAsync(User.Identity.Name,
                HttpContext.RequestAborted);

            var updatedPerson = await personRepository.UpdateAsync(curPersonId, profileData,
                HttpContext.RequestAborted);

            TempData["SuccessMessage"] = "Профиль успешно сохранен";
            return View(mapper.Map<UpdatePersonDto>(updatedPerson));
        }
    }
}
