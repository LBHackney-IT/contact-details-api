using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.E2ETests.Fixtures
{
    public class ContactDetailsFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IDynamoDBContext _dbContext;
        public List<ContactDetailsEntity> Contacts { get; private set; } = new List<ContactDetailsEntity>();

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
                                                                 .With(y => y.CreatedAt, funcDT)
                                                                 .Create())
                           .With(x => x.IsActive, isActive)
                           .With(x => x.TargetType, TargetType.person)
                           .With(x => x.TargetId, TargetId)
                           .CreateMany(count);
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
    }
}
