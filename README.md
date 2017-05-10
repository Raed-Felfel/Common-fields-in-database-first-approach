# Common fields in database first approach
How to treat the common fields in database tables like the create and modification dates. using ASP.NET MVC and Database first approach?

## System requirements

##### The following info to be included in all tables (common fields)
- Deleted
- DeletedAt
- DeletedBy
- CreatedAt
- CreatedBy
- ModifiedAt
- ModifiedBy
##### The values of above fields to managed automatically with least effort of the programmer.
##### The used approach for the project is Database first

## The solution

1.	Create a base class where all data entities will derived from.
2.	Modify the T4 templates to derive the data entities from the base defined class and not including the above common fields inside generated files.
3.	Override the save changes method to treat these fields.
4.	Include the modified data context class in all controllers using a base controller.

## The steps

1.	Create a new “ASP.NET Web Application”.
2.	Select the “MVC” template.
3.	Configure the connection string inside “Web.config” file.
4.	Add a new class call it “EntityBase” ,as example, using the following code:

```c#
public class EntityBase
    {
        public bool? Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
```

5.	Run the application register and login. This will automatically create the OWIN membership tables for you.
6.	Right-click your project  Add  New item, Ctrl + Shift + A.
7.	Choose “ADO.NET Entity Data Model”, name your models the name “Entities”, as example, and press the Add button.
8.	Choose “EF Designer from database” and press the Next button.
9.	Verify the database connection name and continue.
10.	Choose one or more tables and press the finish button.
11.	Open the file Entities.tt
12.	Find the function “EntityClassOpening” and modify the last line to inherit all entities from our previously created class “EntityBase”. Below is the code:

```c#
    public string EntityClassOpening(EntityType entity)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} {1}partial class {2}{3}",
            Accessibility.ForType(entity),
            _code.SpaceAfter(_code.AbstractOption(entity)),
            _code.Escape(entity),
            _code.StringBefore(" : ", _typeMapper.GetTypeName(entity.BaseType) ?? "EntityBase" ));
    }
```

13.	Find the function “GetSimpleProperties” and modify it to the below code:

```c#
    public IEnumerable<EdmProperty> GetSimpleProperties(EntityType type)
    {
        return type.Properties.Where(p => p.TypeUsage.EdmType is SimpleType && p.DeclaringType == type)
		.Where(p => p.Name != "Deleted" 
				&& p.Name != "DeletedBy" 
				&& p.Name != "DeletedAt"
				&& p.Name != "CreatedAt"
				&& p.Name != "CreatedBy"
				&& p.Name != "ModifiedAt"
				&& p.Name != "ModifiedBy");
    }
```

14.	Once you save the template, all entity classes will be re-generated and now they are inherited from our class EntityBase without including the common attributes.
15.	Add a new partial class definition to the Entities class. In this class we’ll define a userid attribute and override the default save changes method:

```c#
    public partial class Entities
    {
        public string UserId { get; set; }

        public override int SaveChanges()
        {
            //changes on tables derived from EntityBase
            var changesSet = ChangeTracker.Entries<EntityBase>();
            if(changesSet != null)
            {
                //new added records
                foreach (var entry in changesSet.Where(e => e.State == System.Data.Entity.EntityState.Added))
                {
                    entry.Entity.Deleted = false;
                    entry.Entity.CreatedBy = UserId;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                //modified records
                foreach (var entry in changesSet.Where(e => e.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.Entity.ModifiedBy = UserId;
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    if (entry.CurrentValues.GetValue<bool?>("Deleted") == true)
                    {
                        entry.Entity.DeletedBy = UserId;
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                    }
                }
            } //if(changesSet != null)
            return base.SaveChanges();
        }
    }
```

16.	Create a new controller; name it “BaseController” that derived from the Controller class to include a database attribute. A constructor to create the database. Then override the BeginExecuteCore method to initialize the database with userid.

```c#
   public class BaseController : Controller
    {
        public Entities db { get; set; }

        public BaseController()
        {
            db = new Entities();
        }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            if(Request.IsAuthenticated)
            {
                var UserManager = HttpContext.GetOwinContext()
                    .GetUserManager<ApplicationUserManager>();

                var user = UserManager.FindById(User.Identity.GetUserId());
                if(user != null)
                {
                    db.UserId = user.Id;
                }
            }
            return base.BeginExecuteCore(callback, state);
        }

    }
```

##### 17.	Add new controller to your project using the “MVC 5 Controller with views, using Entity Framework”. Once the code generated modify the controller 

-	Derive from Basecontroller class
-	Remove the database definition as we have our own one. 
-	Modify the delete action to set Deleted=false instead of hard deletion.
-	Change the Edit to action to the below one:

```c#
  [HttpPost]
  [ValidateAntiForgeryToken]
  public ActionResult Edit(Staff staff)
  {
      if (ModelState.IsValid)
      {
          var dbStaff = db.Staffs.Find(staff.Id);
          if(dbStaff == null)
          {
              return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
          }
          dbStaff.FullName = staff.FullName;
          dbStaff.Title = staff.Title; 
          db.SaveChanges();
          return RedirectToAction("Index");
      }
      return View(staff);
  }
```

18.	Modify the view to remove un-wanted controls of our common fields.
19.	Test the app.
