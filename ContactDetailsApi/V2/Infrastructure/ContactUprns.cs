using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using System;
using System.Collections.Generic;

namespace ContactDetailsApi.V2.Infrastructure
{
    //public class AssetTenure
    //{
    //    public string Uprn { get; set; }
    //    public string TenureId { get; set; }

    //   public List<HouseHoldMemberContact> HouseholdMembers { get; set; }

    //}

    ////public class ContactUprns
    ////{
    ////    public string Uprn { get; set; }
    ////    public List<HouseHoldMemberContact> HouseHoldMembz { get; set; }
    ////}

    //public class HouseHoldMemberContact
    //{
    //    public HouseholdMembers HouseholdMember { get; set; }

        
    //    public List<ContactDetailsEntity> ContactDetails { get; set; }
    //}

    //public class AssetTenure
    //{
    //    public string Uprn { get; set; }
    //    public Guid? TenureId { get; set; }

    //}

    public class ContactByUprn
    {
        public string Uprn { get; set; }
        public Guid? TenureId { get; set; }
        public List<PersonContact> Contacts { get; set; }

    }


    public class PersonContact
    {
        public string PersonTenureType { get; set; }
        public bool IsResponsible { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }

        public List<PersonContactDeets> Deets { get; set; }
    }

    public class PersonContactDeets
    {
        public string ContactType { get; set; }
        public string SubType { get; set; }
        public string Value { get; set; }

    }
}
