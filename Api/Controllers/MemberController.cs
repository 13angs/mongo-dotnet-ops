using Api.Interfaces;
using Api.Paramters;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/members")]
    public class MemberController : ControllerBase
    {
        private readonly IMember _member;
        public MemberController(IMember member)
        {
            _member = member;
        }

        [HttpGet]
        public ActionResult<object> GetMembers([FromQuery] MemberParam param)
        {
            return Ok(_member.GetAllMember(param));
        }
    }
}