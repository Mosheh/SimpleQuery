# SimpleQuery
ORM Very Easy to use. All CRUD operations. Look:


Execute insert, update, delete, select returing strongly typed List
------------------------------------------------------------
```csharp
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
         
    var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
    var scriptBuilder = conn.GetScriptBuild();

    var cliente = new Cliente() { Nome = "Miranda" };

    var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
    conn.Execute(createTableScript);
    var id = conn.InsertRereturnId<Cliente>(cliente);              
```
      
     
     
