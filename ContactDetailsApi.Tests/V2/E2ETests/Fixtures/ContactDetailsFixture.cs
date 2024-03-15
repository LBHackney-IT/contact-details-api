using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Infrastructure;
using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using Hackney.Shared.Tenure.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressExtended = ContactDetailsApi.V2.Domain.AddressExtended;
using ContactInformation = ContactDetailsApi.V2.Domain.ContactInformation;

namespace ContactDetailsApi.Tests.V2.E2ETests.Fixtures
{
    public class ContactDetailsFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        public readonly IDynamoDBContext _dbContext;
        public List<ContactDetailsEntity> Contacts { get; private set; } = new List<ContactDetailsEntity>();

        public ContactDetailsRequestObject ContactRequestObject { get; private set; } = new ContactDetailsRequestObject();

        public EditContactDetailsQuery PatchContactDetailsQuery { get; private set; } = new EditContactDetailsQuery();
        public EditContactDetailsRequest PatchContactRequestObject { get; private set; } = new EditContactDetailsRequest();

        public ContactDetailsEntity ExistingContact { get; private set; } = new ContactDetailsEntity();


        private const int MAX_EMAIL_CONTACTS = 5;
        private const int MAX_PHONE_CONTACTS = 5;

        public Guid TargetId { get; private set; }

        public string InvalidTargetId { get; private set; }

        public readonly List<Action> _cleanup = new List<Action>();


        public ContactDetailsFixture(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var action in _cleanup)
                    action();
                _disposed = true;
            }
        }

        private IEnumerable<ContactDetailsEntity> CreateContacts(int count, bool isActive)
        {
            var contact = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.CreatedBy, () => _fixture.Create<CreatedBy>())
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, isActive)
                .With(x => x.TargetType, TargetType.person)
                .With(x => x.TargetId, TargetId).CreateMany(count);

            Contacts = contact.ToList();

            return contact;
        }

        private IEnumerable<ContactDetailsEntity> CreateContactsForType(ContactType type, Guid targetId, int count, bool isActive = true)
        {
            return _fixture.Build<ContactDetailsEntity>()
                .With(x => x.CreatedBy, () => _fixture.Create<CreatedBy>())
                .With(x => x.ContactInformation, CreateContactInformation(type))
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, isActive)
                .With(x => x.TargetType, TargetType.person)
                .With(x => x.TargetId, targetId).CreateMany(count);
        }

        private ContactDetailsRequestObject CreateContactRestObject(ContactType type = ContactType.email)
        {
            return SetupContactCreationFixture(type);
        }

        private EditContactDetailsRequest CreateEditContactDetailsRequest()
        {
            return _fixture.Create<EditContactDetailsRequest>();
        }

        private EditContactDetailsQuery CreateEditContactDetailsQuery()
        {
            return _fixture.Create<EditContactDetailsQuery>();
        }

        private ContactInformation CreateContactInformation(ContactType type)
        {
            var value = type switch
            {
                ContactType.email => "somone-else@somewhere.com",
                ContactType.phone => "02111231234",
                _ => "some address",
            };
            return _fixture.Build<ContactInformation>()
                           .With(y => y.ContactType, type)
                           .With(y => y.Value, value)
                           .Create();
        }

        private ContactDetailsRequestObject SetupContactCreationFixture(ContactType type = ContactType.email)
        {
            var contactInfo = CreateContactInformation(type);
            return _fixture.Build<ContactDetailsRequestObject>()
                .With(x => x.ContactInformation, contactInfo)
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.TargetType, TargetType.person)
                .With(x => x.TargetId, Guid.NewGuid).Create();
        }

        public async Task GivenMaxContactDetailsAlreadyExist(Guid targetId)
        {
            Contacts.Clear();
            Contacts.AddRange(CreateContactsForType(ContactType.email, targetId, MAX_EMAIL_CONTACTS));
            Contacts.AddRange(CreateContactsForType(ContactType.phone, targetId, MAX_PHONE_CONTACTS));

            foreach (var contact in Contacts)
            {
                await _dbContext.SaveAsync(contact).ConfigureAwait(false);
                _cleanup.Add(async () => await _dbContext.DeleteAsync(contact).ConfigureAwait(false));
            }

        }

        public async Task GivenAContactAlreadyExists()
        {
            ExistingContact = CreateContacts(1, true).First();

            await _dbContext.SaveAsync(ExistingContact).ConfigureAwait(false);
            _cleanup.Add(async () => await _dbContext.DeleteAsync(ExistingContact).ConfigureAwait(false));
        }

        public async Task GivenContactDetailsAlreadyExist(int active, int inactive)
        {
            if (!Contacts.Any())
            {
                TargetId = Guid.NewGuid();

                if (active > 0)
                {
                    Contacts.AddRange(CreateContacts(active, true));
                }

                if (inactive > 0)
                {
                    Contacts.AddRange(CreateContacts(inactive, false));
                }

                foreach (var contact in Contacts)
                {
                    await _dbContext.SaveAsync(contact).ConfigureAwait(false);
                    _cleanup.Add(async () => await _dbContext.DeleteAsync(contact).ConfigureAwait(false));
                }

            }
        }

        public void GivenANewContactRequestWithInvalidAddressLine1WhenTheContactTypeIsAddress()
        {
            ContactRequestObject = CreateContactRestObject();

            var addressExtended = _fixture.Build<AddressExtended>()
               .With(x => x.AddressLine1, "")
               .Create();

            ContactRequestObject.ContactInformation.AddressExtended = addressExtended;
            ContactRequestObject.ContactInformation.ContactType = ContactType.address;
        }

        public void GivenANewContactRequestWithInvalidAddressLine1WhenTheContactTypeNotAddress()
        {
            ContactRequestObject = CreateContactRestObject();

            var addressExtended = _fixture.Build<AddressExtended>()
               .With(x => x.AddressLine1, "")
               .Create();

            ContactRequestObject.ContactInformation.AddressExtended = addressExtended;
            ContactRequestObject.ContactInformation.ContactType = ContactType.email;
        }

        public void GivenANewContactRequestWithInvalidPostCodeWhenTheContactTypeNotAddress()
        {
            ContactRequestObject = CreateContactRestObject();

            var addressExtended = _fixture.Build<AddressExtended>()
               .With(x => x.PostCode, "")
               .Create();

            ContactRequestObject.ContactInformation.AddressExtended = addressExtended;
            ContactRequestObject.ContactInformation.ContactType = ContactType.email;
        }

        public void GivenANewContactRequestWithInvalidPostCodeWhenTheContactTypeIsAddress()
        {
            ContactRequestObject = CreateContactRestObject();

            var addressExtended = _fixture.Build<AddressExtended>()
               .With(x => x.PostCode, (string) null)
               .Create();

            ContactRequestObject.ContactInformation.AddressExtended = addressExtended;
            ContactRequestObject.ContactInformation.ContactType = ContactType.address;
        }

        public void GivenContactDetailsDoesNotExist()
        {
            TargetId = Guid.NewGuid();
        }

        public void GivenANewContactRequest()
        {
            ContactRequestObject = CreateContactRestObject();
        }

        public void GivenAPatchContactRequest(ContactDetailsEntity existingContact = null)
        {
            PatchContactRequestObject = CreateEditContactDetailsRequest();

            if (existingContact == null)
            {
                PatchContactDetailsQuery = CreateEditContactDetailsQuery();
                return;
            }

            PatchContactDetailsQuery = new EditContactDetailsQuery
            {
                ContactDetailId = existingContact.Id,
                PersonId = existingContact.TargetId
            };


        }

        public void GivenANewContactRequest(ContactType type)
        {
            ContactRequestObject = CreateContactRestObject(type);
        }

        public void GivenAnInvalidNewContactRequest()
        {
            ContactRequestObject = new ContactDetailsRequestObject();
        }

        public void GivenANewContactRequestWhereContactTypeIsAddress()
        {
            GivenANewContactRequest();

            ContactRequestObject.ContactInformation.ContactType = ContactType.address;

            var addressExtended = _fixture.Build<AddressExtended>()
               .With(x => x.PostCode, "NW1 6EA")
               .Create();

            ContactRequestObject.ContactInformation.AddressExtended = addressExtended;
        }

        public void GivenANewContactRequestWhereContactTypeIsNotAddress()
        {
            GivenANewContactRequest();

            ContactRequestObject.ContactInformation.ContactType = ContactType.email;

            var addressExtended = _fixture.Build<AddressExtended>()
             .With(x => x.PostCode, "NW1 6EA")
             .Create();

            ContactRequestObject.ContactInformation.AddressExtended = addressExtended;
        }

        public void GivenAnNewContactRequestWithAnInvalidPhoneNumber()
        {
            ContactRequestObject = CreateContactRestObject();
            ContactRequestObject.ContactInformation.ContactType = ContactType.phone;
            ContactRequestObject.ContactInformation.Value = "Something wrong";
        }

        public void GivenAnNewContactRequestWithAnInvalidEmail()
        {
            ContactRequestObject = CreateContactRestObject();
            ContactRequestObject.ContactInformation.ContactType = ContactType.email;
            ContactRequestObject.ContactInformation.Value = "Something wrong";
        }

        public async void GivenAFetchAllContactDetailsByUprnRequest()
        {
            var person = _fixture.Build<PersonDbEntity>()
                .Without(x => x.VersionNumber)
                .Create();
            await _dbContext.SaveAsync(person).ConfigureAwait(false);
            _cleanup.Add(async () => await _dbContext.DeleteAsync(person).ConfigureAwait(false));

            var contactInformation = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.TargetId, person.Id)
                .CreateMany(2)
                .ToList();
            foreach (var contact in contactInformation)
            {
                await _dbContext.SaveAsync(contact).ConfigureAwait(false);
                _cleanup.Add(async () => await _dbContext.DeleteAsync(contact).ConfigureAwait(false));
            }
            Contacts.AddRange(contactInformation);

            var tenant = _fixture.Build<HouseholdMembers>()
                                                 .With(x => x.Id, person.Id)
                                                 .With(x => x.IsResponsible, true)
                                                 .Create();

            var householdMember = _fixture.Build<HouseholdMembers>()
                .With(x => x.IsResponsible, false)
                .Create();

            var householdMembers = new List<HouseholdMembers> { tenant, householdMember };

            var tenure = _fixture.Build<TenureInformationDb>()
                .Without(x => x.VersionNumber)
                .Without(x => x.EndOfTenureDate)
                .With(x => x.HouseholdMembers, householdMembers)
                .Create();

            await _dbContext.SaveAsync(tenure).ConfigureAwait(false);
            _cleanup.Add(async () => await _dbContext.DeleteAsync(tenure).ConfigureAwait(false));

            var inactiveTenure = _fixture.Build<TenureInformationDb>()
                .Without(x => x.VersionNumber)
                .With(x => x.EndOfTenureDate, DateTime.Today.AddDays(-10))
                .Create();

            await _dbContext.SaveAsync(inactiveTenure).ConfigureAwait(false);
            _cleanup.Add(async () => await _dbContext.DeleteAsync(inactiveTenure).ConfigureAwait(false));
        }
    }
}
