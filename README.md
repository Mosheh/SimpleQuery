# SimpleQuery
ORM Very Easy to use. All CRUD operations. Look:

Exemple:
    
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
    
     //Ms Sql Server
     var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString);
     var scriptBuilder = conn.GetScriptBuild();

     var cliente = new Cliente() { Nome = "Miranda" };

     var createTableScript = scriptBuilder.GetCreateTableCommand<Cliente>(cliente);
     conn.Execute(createTableScript);
     var id = conn.InsertRereturnId<Cliente>(cliente);
     
     //SAP Hana
      var conn = new HanaConnection(ConfigurationManager.ConnectionStrings["hana"].ConnectionString);
      var scriptBuilder = conn.GetScriptBuild();
      var cliente = new Cliente() { Nome = "Miranda" };      
      var id = conn.InsertRereturnId<Cliente>(cliente);
      
      
     
     
