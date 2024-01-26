namespace MTCG.Repositories
{
    public interface IDatabaseHelperRepository
    {
        void DropTables(List<string> tableNames);
        void CreateTables();

    }
}
