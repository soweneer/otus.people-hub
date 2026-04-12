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
    public class PersonController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public PersonController(IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity is null)
            {
                return Unauthorized();
            }

            var persons = await _mediator.Send(
                new GetAllRequest(User.Identity.Name), HttpContext.RequestAborted);

            return View(persons);
        }

        [HttpGet]
        public async Task<IActionResult> Person(int personId)
        {
            if (User.Identity is null) return Unauthorized();

            var curPersonId = await _mediator.Send(new FindByEmailRequest(User.Identity.Name),
                HttpContext.RequestAborted);
            var personInfo = await _mediator.Send(new GetRequest(personId, curPersonId), HttpContext.RequestAborted);
            if (personInfo == null)
                throw new UnknownPersonException(personId);

            return View(personInfo);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity is null) return Unauthorized();
            var curPersonId = await _mediator.Send(new FindByEmailRequest(User.Identity.Name),
                HttpContext.RequestAborted);

            var personInfo = await _mediator.Send(new GetRequest(curPersonId), HttpContext.RequestAborted);
            return View(_mapper.Map<UpdatePersonDto>(personInfo));
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdatePersonDto profileData)
        {
            if (User.Identity is null) return Unauthorized();
            if (!ModelState.IsValid)
                return View();
            var curPersonId = await _mediator.Send(new FindByEmailRequest(User.Identity.Name),
                HttpContext.RequestAborted);

            var updatedPerson = await _mediator.Send(new UpdateRequest(curPersonId, profileData),
                HttpContext.RequestAborted);

            TempData["SuccessMessage"] = "Профиль успешно сохранен";
            return View(_mapper.Map<UpdatePersonDto>(updatedPerson));
        }
    }
}
