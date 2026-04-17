using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Exceptions;
using PeopleHub.Extensions;
using PeopleHub.Domain.Model.Dto.Person;
using PeopleHub.Domain.Services;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class PersonController(IPersonService personService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var persons = await personRepository.GetFriendsAsync(
                User.Identity.Name, HttpContext.RequestAborted);

            return View(persons);
        }

        [HttpGet]
        public async Task<IActionResult> Person(int personId)
        {
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
            var curPersonId = await personRepository.GetPersonIdAsync(User.Identity.Name,
                HttpContext.RequestAborted);

            var personInfo = await personRepository.GetByIdAsync(curPersonId, null, HttpContext.RequestAborted);
            return View(personInfo);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdatePersonRequest updateRequest)
        {
            if (!ModelState.IsValid)
                return View();

            var updatedPerson = await personService.UpdateAsync(User.Identity.Name, updateRequest.ToPersonData(), 
                HttpContext.RequestAborted);

            TempData["SuccessMessage"] = "Профиль успешно сохранен";
            return View(updatedPerson);
        }
    }
}
