using Api.Paramters;
using MongoDB.Bson;

namespace Api.Interfaces
{
    public interface IMember
    {
        public string GetAllMember(MemberParam param);
    }
}