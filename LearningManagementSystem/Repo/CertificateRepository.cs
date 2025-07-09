using DocumentFormat.OpenXml.Spreadsheet;
using LearningManagementSystem.Data;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Colors = QuestPDF.Helpers.Colors;

namespace LearningManagementSystem.Repo
{
    public class CertificateRepository : Repository<Certificate>, ICertificateRepository
    {
        public CertificateRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Certificate> GetByEnrollmentIdAsync(int enrollmentId)
        {
            return await _context.Certificates
                .FirstOrDefaultAsync(c => c.EnrollmentId == enrollmentId);
        }

        //public async Task<Certificate> GetByVerificationCodeAsync(string verificationCode)
        //{
        //    return await _context.Certificates
        //        .Include(c => c.Enrollment)
        //            .ThenInclude(e => e.Student)
        //        .Include(c => c.Enrollment)
        //            .ThenInclude(e => e.Course)
        //        .FirstOrDefaultAsync(c => c.VerificationCode == verificationCode);
        //}
        public async Task<Certificate> GenerateCertificatePdf(int enrollmentId)
        {
            var enrollment = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefault(e => e.Id == enrollmentId);
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(50);
                    page.Background(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(20));

                    page.Header()
                        .AlignCenter()
                        .Text("Certificate of Completion")
                        .Bold().FontSize(36).FontColor(Colors.Blue.Darken3);

                    page.Content()
                        .PaddingVertical(40)
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text($"This is to certify that").FontSize(24);
                            col.Item().AlignCenter().Text(enrollment.Student.FullName).Bold().FontSize(32);
                            col.Item().AlignCenter().Text($"has successfully completed the course").FontSize(24);
                            col.Item().AlignCenter().Text(enrollment.Course.Title).Bold().FontSize(28);
                            col.Item().AlignCenter().Text($"on {DateTime.Now:MMMM dd, yyyy}").FontSize(24);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Signed: ").FontColor(Colors.Grey.Darken1);
                            text.Span("________________________").Bold();
                        });
                });
            });
            Certificate certificate = new Certificate
            {
                EnrollmentId = enrollmentId,
                IssuedDate = DateTime.Now,
                Document = document.GeneratePdf()
            };
            await _context.Certificates.AddAsync(certificate);
            await _context.SaveChangesAsync();
            return certificate;
        }
    }
}
