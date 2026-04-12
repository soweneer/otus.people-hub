using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Exceptions;
using PeopleHub.Shared.BusinessLogic.Person;
using PeopleHub.Shared.Model.Dto.Person;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class PersonController(IMapper mapper, IMediator mediator) : Controller
    {

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity is null)
            {
                return Unauthorized();
            }

            var persons = await mediator.Send(
                new GetAllRequest(User.Identity.Name), HttpContext.RequestAborted);

            return View(persons);
        }

        [HttpGet]
        public async Task<IActionResult> Person(int personId)
        {
            if (User.Identity is null) return Unauthorized();

            var curPersonId = await mediator.Send(new FindByEmailRequest(User.Identity.Name),
                HttpContext.RequestAborted);
            var personInfo = await mediator.Send(new GetRequest(personId, curPersonId), HttpContext.RequestAborted);
            if (personInfo == null)
                throw new UnknownPersonException(personId);

            return View(personInfo);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity is null) return Unauthorized();
            var curPersonId = await mediator.Send(new FindByEmailRequest(User.Identity.Name),
                HttpContext.RequestAborted);

            var personInfo = await mediator.Send(new GetRequest(curPersonId), HttpContext.RequestAborted);
            return View(mapper.Map<UpdatePersonDto>(personInfo));
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdatePersonDto profileData)
        {
            if (User.Identity is null) return Unauthorized();
            if (!ModelState.IsValid)
                return View();
            var curPersonId = await mediator.Send(new FindByEmailRequest(User.Identity.Name),
                HttpContext.RequestAborted);

            var updatedPerson = await mediator.Send(new UpdateRequest(curPersonId, profileData),
                HttpContext.RequestAborted);

            TempData["SuccessMessage"] = "Профиль успешно сохранен";
            return View(mapper.Map<UpdatePersonDto>(updatedPerson));
        }
    }
}
