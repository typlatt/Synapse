using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Synapse.SignalBooster.Services;

namespace Synapse.SignalBooster.Tests
{
    public class PhysicianNoteReaderTests
    {
        private readonly PhysicianNoteReader _reader;

        public PhysicianNoteReaderTests()
        {
            _reader = new PhysicianNoteReader(NullLogger<PhysicianNoteReader>.Instance);
        }

        [Fact]
        public async Task ReadNoteAsync_WithValidFile_ReturnsSuccessWithContent()
        {
            // Arrange
            string notePath = "/Users/tplatt/Code/Synapse/notes/physician_note1.txt";

            // Act
            var result = await _reader.ReadNoteAsync(notePath);

            // Assert
            result.IsSuccess.Should().BeTrue("physician_note1.txt should exist in notes folder");
            result.Value.Should().NotBeNullOrWhiteSpace("file should contain physician note content");
            result.Value.Should().Contain("Harold Finch", "note contains patient name");
            result.Value.Should().Contain("oxygen", "note describes oxygen equipment");
        }
    }
}
