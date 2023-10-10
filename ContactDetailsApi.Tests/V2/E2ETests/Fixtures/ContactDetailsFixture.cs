using Amazon.DynamoDBv2.DataModel;
using Amazon.SimpleNotificationService;
using AutoFixture;
using AutoFixture.Dsl;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Infrastructure;
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
            return _fixture.Build<ContactDetailsEntity>()
                .With(x => x.CreatedBy, () => _fixture.Create<CreatedBy>())
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, isActive)
                .With(x => x.TargetType, TargetType.person)
                .With(x => x.TargetId, TargetId).CreateMany(count);
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
            return SetupContactCreationFixture(type).Create();
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

        private IPostprocessComposer<ContactDetailsRequestObject> SetupContactCreationFixture(ContactType type = ContactType.email)
        {
            var contactInfo = CreateContactInformation(type);
            return _fixture.Build<ContactDetailsRequestObject>()
                .With(x => x.ContactInformation, contactInfo)
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.TargetType, TargetType.person)
                .With(x => x.TargetId, Guid.NewGuid);
        }

        public async Task GivenMaxContactDetailsAlreadyExist(Guid targetId)
        {
            Contacts.Clear();
            Contacts.AddRange(CreateContactsForType(ContactType.email, targetId, MAX_EMAIL_CONTACTS));
            Contacts.AddRange(CreateContactsForType(ContactType.phone, targetId, MAX_PHONE_CONTACTS));

            foreach (var contact in Contacts)
                await _dbContext.SaveAsync(contact).ConfigureAwait(false);
        }

        public async Task GivenAContactAlreadyExists()
        {
            ExistingContact = CreateContacts(1, true).First();

            await _dbContext.SaveAsync(ExistingContact).ConfigureAwait(false);
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
    }
}
