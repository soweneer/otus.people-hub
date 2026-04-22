using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Model;
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
            var persons = await personService.GetAllAsync(User.Identity!.Name, HttpContext.RequestAborted);

            return View(persons);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(SearchPersonRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SearchError = string.Join("; ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View("Index", Array.Empty<PersonInfo>());
            }

            var firstName = request.FirstName.Trim();
            var lastName = request.LastName.Trim();
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            {
                ViewBag.SearchError = "Заполните хотя бы одно из полей: имя или фамилия";
                return View("Index", Array.Empty<PersonInfo>());
            }

            ViewBag.SearchFirstName = firstName;
            ViewBag.SearchLastName = lastName;
            var persons = await personService.SearchAsync(User.Identity!.Name, 
                firstName, lastName, HttpContext.RequestAborted);

            return View("Index", persons);
        }

        [HttpGet]
        public async Task<IActionResult> Person(int personId)
        {
            var personInfo = await personService.GetByEmailAsync(User.Identity!.Name, personId, HttpContext.RequestAborted);

            return personInfo == null 
                ? throw new UnknownPersonException(personId)
                : View(personInfo);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var personInfo = await personService.GetProfileAsync(User.Identity!.Name, HttpContext.RequestAborted);

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
