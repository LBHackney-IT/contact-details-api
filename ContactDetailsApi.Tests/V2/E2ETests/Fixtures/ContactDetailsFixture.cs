using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Infrastructure;
using Hackney.Shared.Asset.Infrastructure;
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
                if (Contacts.Any())
                    foreach (var contact in Contacts)
                        _dbContext.DeleteAsync(contact).GetAwaiter().GetResult();

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
            string value;
            switch (type)
            {
                case ContactType.email: value = "somone-else@somewhere.com"; break;
                case ContactType.phone: value = "02111231234"; break;
                default: value = "some address"; break;
            }
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

            var contactInformation = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.TargetId, person.Id)
                .CreateMany(2)
                .ToList();
            Contacts = contactInformation;
            foreach (var contact in contactInformation)
            {
                await _dbContext.SaveAsync(contact).ConfigureAwait(false);
                _cleanup.Add(async () => await _dbContext.DeleteAsync(contact).ConfigureAwait(false));
            }


            var tenure = _fixture.Build<TenureInformationDb>()
                .Without(x => x.VersionNumber)
                .With(x => x.HouseholdMembers,
                    _fixture.Build<HouseholdMembers>()
                    .With(x => x.Id, person.Id)
                    .CreateMany(1)
                    .ToList()
                )
                .Create();

            await _dbContext.SaveAsync(tenure).ConfigureAwait(false);

            var asset = _fixture.Build<AssetDb>()
                .Without(x => x.VersionNumber)
                .Create();

            asset.Tenure.Id = tenure.Id.ToString();

            await _dbContext.SaveAsync(asset).ConfigureAwait(false);

            _cleanup.Add(async () => await _dbContext.DeleteAsync(tenure).ConfigureAwait(false));
            _cleanup.Add(async () => await _dbContext.DeleteAsync(asset).ConfigureAwait(false));
            _cleanup.Add(async () => await _dbContext.DeleteAsync(person).ConfigureAwait(false));
        }
    }
}
