using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Exceptions;
using PeopleHub.Lib.BusinessLogic.Person.FindByEmail;
using PeopleHub.Lib.Model.Dto.Person;

namespace PeopleHub.Controllers
{
    using FindPersonByEmailRequest = Request;
    using GetPersonRequest = Lib.BusinessLogic.Person.Get.Request;
    using UpdatePersonRequest = Lib.BusinessLogic.Person.Update.Request;
    using GetAllPersonsRequest = Lib.BusinessLogic.Person.GetAll.Request;
    
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
                new GetAllPersonsRequest(User.Identity.Name), HttpContext.RequestAborted);
            
            return View(persons);
        }

        [HttpGet]
        public async Task<IActionResult> Person(int personId)
        {
            if (User.Identity is null) return Unauthorized();
            
            var curPersonId = await _mediator.Send(new FindPersonByEmailRequest(User.Identity.Name),
                HttpContext.RequestAborted);
            var personInfo = await _mediator.Send(new GetPersonRequest(personId, curPersonId), HttpContext.RequestAborted);
            if (personInfo == null)
                throw new UnknownPersonException(personId);
            
            return View(personInfo);
        }
        
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity is null) return Unauthorized();
            var curPersonId = await _mediator.Send(new FindPersonByEmailRequest(User.Identity.Name),
                HttpContext.RequestAborted);

            var personInfo = await _mediator.Send(new GetPersonRequest(curPersonId), HttpContext.RequestAborted);
            return View(_mapper.Map<DtoUpdatePerson>(personInfo));
        }

        [HttpPost]
        public async Task<IActionResult> Profile(DtoUpdatePerson profileData)
        {
            if (User.Identity is null) return Unauthorized();
            if (!ModelState.IsValid)
                return View();
            var curPersonId = await _mediator.Send(new FindPersonByEmailRequest(User.Identity.Name),
                HttpContext.RequestAborted);

            var updatedPerson = await _mediator.Send(new UpdatePersonRequest(curPersonId, profileData),
                HttpContext.RequestAborted);

            TempData["SuccessMessage"] = "Профиль успешно сохранен";
            return View(_mapper.Map<DtoUpdatePerson>(updatedPerson));
        }
    }
}