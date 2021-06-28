using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Dsl;
using ContactDetailsApi.V1.Boundary.Request;

namespace ContactDetailsApi.Tests.V1.E2ETests.Fixtures
{
    public class ContactDetailsFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        public readonly IDynamoDBContext _dbContext;
        public List<ContactDetailsEntity> Contacts { get; private set; } = new List<ContactDetailsEntity>();
        public ContactDetailsRequestObject Contact { get; private set; } = new ContactDetailsRequestObject();

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
            var random = new Random();
            Func<DateTime> funcDT = () => DateTime.UtcNow.AddDays(0 - random.Next(100));

            return _fixture.Build<ContactDetailsEntity>()
                .With(x => x.CreatedBy, () => _fixture.Build<CreatedBy>()
                    .Create())
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, isActive)
                .With(x => x.TargetType, TargetType.person)
                .With(x => x.TargetId, TargetId).CreateMany(count);
        }

        private ContactDetailsRequestObject CreateContact(bool isActive)
        {
            return SetupContactCreationFixture(isActive).Create();
        }

        private IPostprocessComposer<ContactDetailsRequestObject> SetupContactCreationFixture(bool isActive)
        {
            var random = new Random();
            Func<DateTime> funcDT = () => DateTime.UtcNow.AddDays(0 - random.Next(100));

            return _fixture.Build<ContactDetailsRequestObject>()
                .With(x => x.CreatedBy, () => _fixture.Build<CreatedBy>()
                    .With(y => y.EmailAddress, "somone@somewhere.com")
                    .Create())
                .With(x => x.ContactInformation, () => _fixture.Build<ContactInformation>()
                    .With(y => y.ContactType, ContactType.email)
                    .With(y => y.Value, "somone-else@somewhere.com")
                    .Create())
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, isActive)
                .With(x => x.TargetType, TargetType.person)
                .With(x => x.TargetId, Guid.NewGuid);
        }


        public async Task GivenContactDetailsAlreadyExist(int active, int inactive)
        {
            if (!Contacts.Any())
            {
                TargetId = Guid.NewGuid();

                if (active > 0)
                    Contacts.AddRange(CreateContacts(active, true));

                if (inactive > 0)
                    Contacts.AddRange(CreateContacts(inactive, false));

                foreach (var note in Contacts)
                    await _dbContext.SaveAsync(note).ConfigureAwait(false);
            }
        }

        public void GivenContactDetailsDoesNotExist()
        {
            TargetId = Guid.NewGuid();
        }

        public void GivenANewContactRequest()
        {
            Contact = CreateContact(true);
        }

        public void GivenAnInvalidNewContactRequest()
        {
            Contact = new ContactDetailsRequestObject();
        }
    }
}
