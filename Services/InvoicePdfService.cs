using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Services
{
    public interface IInvoicePdfService
    {
        byte[] BuildTechnicianInvoicePdf(ApplicationUser technician, string invoiceNumber, DateTime invoiceDate, DateTime dueDate, decimal amountAll);
        byte[] BuildBusinessInvoicePdf(Business business, string invoiceNumber, DateTime invoiceDate, DateTime dueDate, decimal amountAll);
    }

    public class InvoicePdfService : IInvoicePdfService
    {
        public InvoicePdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] BuildTechnicianInvoicePdf(ApplicationUser technician, string invoiceNumber, DateTime invoiceDate, DateTime dueDate, decimal amountAll)
        {
            var fullName = technician.GetFullName();
            var email = technician.Email ?? "-";
            var city = string.IsNullOrWhiteSpace(technician.City) ? "-" : technician.City;
            var company = string.IsNullOrWhiteSpace(technician.CompanyName) ? "-" : technician.CompanyName;
            var phone = string.IsNullOrWhiteSpace(technician.PhoneNumber) ? "-" : technician.PhoneNumber;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("TEKNOSOS - FATURE MUAJORE").Bold().FontSize(18);
                        col.Item().Text($"Nr. Fatures: {invoiceNumber}").FontSize(11);
                        col.Item().Text($"Data e leshimit: {invoiceDate:dd/MM/yyyy}").FontSize(11);
                        col.Item().Text($"Afati i pageses: {dueDate:dd/MM/yyyy}").FontSize(11);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Text("Detajet e Teknikut").Bold().FontSize(13);
                        col.Item().Text($"Emri: {fullName}");
                        col.Item().Text($"Email: {email}");
                        col.Item().Text($"Telefon: {phone}");
                        col.Item().Text($"Qyteti: {city}");
                        col.Item().Text($"Kompania: {company}");

                        col.Item().PaddingTop(12).Text("Detajet e Abonimit").Bold().FontSize(13);
                        col.Item().Text("Plani: Teknik - Standard");
                        col.Item().Text("Kufiri i preventivave: 20 / muaj");
                        col.Item().Text($"Shuma mujore: {amountAll:N0} ALL").Bold();

                        col.Item().PaddingTop(12).Text("Shenim").Bold().FontSize(12);
                        col.Item().Text("Per version pa limit preventivash, kontaktoni administratorin e platformes.");
                    });

                    page.Footer().AlignCenter().Text("TeknoSOS - Platforma e Sherbimeve Teknike ne Shqiperi").FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf();
        }

        public byte[] BuildBusinessInvoicePdf(Business business, string invoiceNumber, DateTime invoiceDate, DateTime dueDate, decimal amountAll)
        {
            var name = business.Name;
            var email = string.IsNullOrWhiteSpace(business.ContactEmail) ? "-" : business.ContactEmail;
            var city = string.IsNullOrWhiteSpace(business.City) ? "-" : business.City;
            var nipt = string.IsNullOrWhiteSpace(business.NIPT) ? "-" : business.NIPT;
            var phone = string.IsNullOrWhiteSpace(business.ContactPhone) ? "-" : business.ContactPhone;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("TEKNOSOS - FATURE MUAJORE BIZNESI").Bold().FontSize(18);
                        col.Item().Text($"Nr. Fatures: {invoiceNumber}").FontSize(11);
                        col.Item().Text($"Data e leshimit: {invoiceDate:dd/MM/yyyy}").FontSize(11);
                        col.Item().Text($"Afati i pageses: {dueDate:dd/MM/yyyy}").FontSize(11);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Text("Detajet e Biznesit").Bold().FontSize(13);
                        col.Item().Text($"Emri i biznesit: {name}");
                        col.Item().Text($"NIPT: {nipt}");
                        col.Item().Text($"Email: {email}");
                        col.Item().Text($"Telefon: {phone}");
                        col.Item().Text($"Qyteti: {city}");

                        col.Item().PaddingTop(12).Text("Detajet e Abonimit").Bold().FontSize(13);
                        col.Item().Text("Plani: Biznes - Materiale dhe shitje");
                        col.Item().Text("Kufiri i preventivave: 20 / muaj");
                        col.Item().Text($"Shuma mujore: {amountAll:N0} ALL").Bold();

                        col.Item().PaddingTop(12).Text("Shenim").Bold().FontSize(12);
                        col.Item().Text("Per version pa limit preventivash, kontaktoni administratorin e platformes.");
                    });

                    page.Footer().AlignCenter().Text("TeknoSOS - Platforma e Sherbimeve Teknike ne Shqiperi").FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf();
        }
    }
}
