Remove-Item -Force -Recurse .\ContraCoreDB -ErrorAction SilentlyContinue
dotnet ef dbcontext scaffold "Server=127.0.0.1;Port=5432;User Id=contracore_usr;Password=EvPvkro59Jb7RK3o;Database=contradb;" `
    Npgsql.EntityFrameworkCore.PostgreSQL `
    --schema contracore `
    -o ContraCoreDB `
    -c ContraCoreDBContext `
    --use-database-names

Remove-Item -Force -Recurse .\ContraWebDB -ErrorAction SilentlyContinue
dotnet ef dbcontext scaffold "Server=127.0.0.1;Port=5432;User Id=contraweb_usr;Password=U475jBKZfK3xhbVZ;Database=contradb;" `
    Npgsql.EntityFrameworkCore.PostgreSQL `
    --schema contraweb `
    -o ContraWebDB `
    -c ContraWebDBContext `
    --use-database-names
