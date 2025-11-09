using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Synapse.SignalBooster.Models;

namespace Synapse.SignalBooster.Services
{
    /// <summary>
    /// Extracts structured DME information from physician notes.
    /// </summary>
    public class DmeExtractor
    {
        private readonly ILogger _logger;

        public DmeExtractor(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Extracts DME requirements from a physician note.
        /// </summary>
        /// <param name="noteContent">The raw physician note text</param>
        /// <returns>Structured DME extraction data</returns>
        public DmeExtraction Extract(string noteContent)
        {
            if (string.IsNullOrWhiteSpace(noteContent))
            {
                throw new ArgumentException("Note content cannot be empty", nameof(noteContent));
            }

            var extraction = new DmeExtraction
            {
                Device = ExtractDeviceType(noteContent),
                OrderingProvider = ExtractOrderingProvider(noteContent),
                PatientName = ExtractPatientName(noteContent),
                Dob = ExtractDob(noteContent),
                Diagnosis = ExtractDiagnosis(noteContent)
            };

            // Device-specific extraction
            switch (extraction.Device)
            {
                case "CPAP":
                    ExtractCpapDetails(noteContent, extraction);
                    break;
                case "Oxygen Tank":
                    ExtractOxygenDetails(noteContent, extraction);
                    break;
                case "Wheelchair":
                    // Future: Add wheelchair-specific details
                    break;
            }

            _logger.LogDebug("Extraction complete: {@Extraction}", extraction);
            return extraction;
        }

        private string ExtractDeviceType(string noteContent)
        {
            if (noteContent.Contains("CPAP", StringComparison.OrdinalIgnoreCase))
                return "CPAP";
            if (noteContent.Contains("oxygen", StringComparison.OrdinalIgnoreCase))
                return "Oxygen Tank";
            if (noteContent.Contains("wheelchair", StringComparison.OrdinalIgnoreCase))
                return "Wheelchair";

            _logger.LogWarning("No recognized device type found in note");
            return "Unknown";
        }

        private string ExtractOrderingProvider(string noteContent)
        {
            int drIndex = noteContent.IndexOf("Dr.", StringComparison.OrdinalIgnoreCase);
            if (drIndex < 0)
            {
                _logger.LogWarning("No ordering provider found in note");
                return "Unknown";
            }

            // Extract provider name after "Dr."
            string remaining = noteContent.Substring(drIndex);
            remaining = remaining.Replace("Ordered by ", "", StringComparison.OrdinalIgnoreCase)
                                 .Replace("Ordering Physician:", "", StringComparison.OrdinalIgnoreCase)
                                 .Trim();

            // Take first line or sentence
            int newlineIndex = remaining.IndexOfAny(new[] { '\n', '\r' });
            if (newlineIndex > 0)
                remaining = remaining.Substring(0, newlineIndex);

            return remaining.Trim('.', ' ');
        }

        private void ExtractCpapDetails(string noteContent, DmeExtraction extraction)
        {
            // Mask type
            if (noteContent.Contains("full face", StringComparison.OrdinalIgnoreCase))
            {
                extraction.MaskType = "full face";
            }

            // Add-ons
            if (noteContent.Contains("humidifier", StringComparison.OrdinalIgnoreCase))
            {
                extraction.AddOns?.Add("humidifier");
            }

            // Qualifier (AHI score)
            if (noteContent.Contains("AHI", StringComparison.OrdinalIgnoreCase))
            {
                Match ahiMatch = Regex.Match(noteContent, @"AHI[:\s>]+(\d+)", RegexOptions.IgnoreCase);
                if (ahiMatch.Success)
                {
                    extraction.Qualifier = $"AHI > {ahiMatch.Groups[1].Value}";
                }
                else if (noteContent.Contains("AHI > 20"))
                {
                    extraction.Qualifier = "AHI > 20";
                }
            }
        }

        private void ExtractOxygenDetails(string noteContent, DmeExtraction extraction)
        {
            // Oxygen flow rate (liters per minute)
            Match litersMatch = Regex.Match(noteContent, @"(\d+(?:\.\d+)?)\s*L", RegexOptions.IgnoreCase);
            if (litersMatch.Success)
            {
                extraction.Liters = $"{litersMatch.Groups[1].Value} L";
            }

            // Usage pattern
            bool hasSleep = noteContent.Contains("sleep", StringComparison.OrdinalIgnoreCase);
            bool hasExertion = noteContent.Contains("exertion", StringComparison.OrdinalIgnoreCase);

            if (hasSleep && hasExertion)
                extraction.Usage = "sleep and exertion";
            else if (hasSleep)
                extraction.Usage = "sleep";
            else if (hasExertion)
                extraction.Usage = "exertion";
        }

        private string? ExtractPatientName(string noteContent)
        {
            // Look for "Patient Name:" pattern
            Match match = Regex.Match(noteContent, @"Patient\s+Name:\s*(.+?)(?:\n|$)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            return null;
        }

        private string? ExtractDob(string noteContent)
        {
            // Look for "DOB:" pattern
            Match match = Regex.Match(noteContent, @"DOB:\s*(.+?)(?:\n|$)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            return null;
        }

        private string? ExtractDiagnosis(string noteContent)
        {
            // Look for "Diagnosis:" pattern
            Match match = Regex.Match(noteContent, @"Diagnosis:\s*(.+?)(?:\n|$)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            return null;
        }
    }
}