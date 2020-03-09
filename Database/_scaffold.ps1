Remove-Item -Force -Recurse .\ContraDB -ErrorAction SilentlyContinue
dotnet ef dbcontext scaffold "Server=127.0.0.1;Port=5432;User Id=contra_usr;Password=EvPvkro59Jb7RK3o;Database=contradb;" `
    Npgsql.EntityFrameworkCore.PostgreSQL `
    -o ContraDB `
    -c ContraDBContext `
    --use-database-names
