using System;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using Synapse.SignalBooster.Models;
using Synapse.SignalBooster.Services;

namespace Synapse.SignalBooster.Tests
{
    public class DmeExtractorTests
    {
        private readonly DmeExtractor _extractor;

        public DmeExtractorTests()
        {
            _extractor = new DmeExtractor(NullLogger.Instance);
        }

        [Fact]
        public void Extract_WithCpapNote_ExtractsCpapDevice()
        {
            // Arrange
            string note = "Patient needs a CPAP with full face mask and humidifier. AHI > 20. Ordered by Dr. Cameron.";

            // Act
            DmeExtraction result = _extractor.Extract(note);

            // Assert
            result.Device.Should().Be("CPAP");
            result.MaskType.Should().Be("full face");
            result.AddOns.Should().Contain("humidifier");
            result.Qualifier.Should().Be("AHI > 20");
            result.OrderingProvider.Should().Contain("Dr. Cameron");
        }

        [Fact]
        public void Extract_WithOxygenNote_ExtractsOxygenDetails()
        {
            // Arrange
            string note = @"Patient Name: Harold Finch
DOB: 04/12/1952
Diagnosis: COPD
Prescription: Requires a portable oxygen tank delivering 2 L per minute.
Usage: During sleep and exertion.
Ordering Physician: Dr. Cuddy";

            // Act
            DmeExtraction result = _extractor.Extract(note);

            // Assert
            result.Device.Should().Be("Oxygen Tank");
            result.Liters.Should().Be("2 L");
            result.Usage.Should().Be("sleep and exertion");
            result.OrderingProvider.Should().Contain("Dr. Cuddy");
        }

        [Fact]
        public void Extract_WithWheelchairNote_ExtractsWheelchair()
        {
            // Arrange
            string note = "Patient requires a wheelchair for mobility. Ordered by Dr. House.";

            // Act
            DmeExtraction result = _extractor.Extract(note);

            // Assert
            result.Device.Should().Be("Wheelchair");
            result.OrderingProvider.Should().Contain("Dr. House");
        }

        [Fact]
        public void Extract_WithEmptyNote_ThrowsArgumentException()
        {
            // Act & Assert
            Action act = () => _extractor.Extract("");
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Note content cannot be empty*");
        }

        [Fact]
        public void Extract_WithNoRecognizedDevice_ReturnsUnknown()
        {
            // Arrange
            string note = "Patient needs some medical equipment. Ordered by Dr. Smith.";

            // Act
            DmeExtraction result = _extractor.Extract(note);

            // Assert
            result.Device.Should().Be("Unknown");
        }

        [Fact]
        public void Extract_WithNoProvider_ReturnsUnknown()
        {
            // Arrange
            string note = "Patient needs a CPAP machine.";

            // Act
            DmeExtraction result = _extractor.Extract(note);

            // Assert
            result.OrderingProvider.Should().Be("Unknown");
        }
    }
}
