namespace B2BStore.Settings.Api.Dtos
{
    // Satır seçicide her satırı tanıtmak için kısa özet
    public class RowSummaryDto
    {
        // Kaçıncı satır (0 = 1. satır)
        public int Row { get; set; }

        // Satırın logosu (ör. "fcs-logo.jpg")
        public string? Logo { get; set; }

        // Satırın tema rengi
        public string? LayoutColor { get; set; }

        // Bayi adı
        public string? DealerName { get; set; }
    }
}
