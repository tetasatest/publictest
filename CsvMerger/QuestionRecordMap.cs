using CsvHelper.Configuration;

namespace CsvMerger
{
    public sealed class QuestionRecordMap : ClassMap<QuestionRecord>
    {
        public QuestionRecordMap()
        {
            Map(m => m.Question).Index(0).Name("Pytanie");
            Map(m => m.Answer).Index(1).Name("Odpowiedź");
            Map(m => m.LegalBasis).Index(2).Name("Podstawa prawna");
        }
    }
}