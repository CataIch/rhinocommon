using System;
using Rhino;
using System.Runtime.InteropServices;

namespace examples_cs
{
  // You must define a Guid attribute for your user data derived class
  // in order to support serialization. Every custom user data class
  // needs a custom Guid
  [Guid("DAAA9791-01DB-4F5F-B89B-4AE46767C783")]
  public class PhysicalData : Rhino.DocObjects.Custom.UserData
  {
    public int Weight{ get; set; }
    public double Density {get; set;}


    // Your UserData class must have a public parameterless constructor
    public PhysicalData(){}

    public PhysicalData(int weight, double density)
    {
      Weight = weight;
      Density = density;
    }

    public override string Description
    {
      get { return "Physical Properties"; }
    }

    public override string ToString()
    {
      return String.Format("weight={0}, density={1}", Weight, Density);
    }

    protected override void OnDuplicate(Rhino.DocObjects.Custom.UserData source)
    {
      PhysicalData src = source as PhysicalData;
      if (src != null)
      {
        Weight = src.Weight;
        Density = src.Density;
      }
    }

    // return true if you have information to save
    public override bool ShouldWrite
    {
      get
      {
        if (Weight > 0 && Density > 0)
          return true;
        return false;
      }
    }

    protected override bool Read(Rhino.FileIO.BinaryArchiveReader archive)
    {
      Rhino.Collections.ArchivableDictionary dict = archive.ReadDictionary();
      if (dict.ContainsKey("Weight") && dict.ContainsKey("Density"))
      {
        Weight = (int)dict["Weight"];
        Density = (double)dict["Density"];
      }
      return true;
    }
    protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
    {
      // you can implement File IO however you want... but the dictionary class makes
      // issues like versioning in the 3dm file a bit easier.  If you didn't want to use
      // the dictionary for writing, your code would look something like.
      //
      //  archive.Write3dmChunkVersion(1, 0);
      //  archive.WriteInt(Weight);
      //  archive.WriteDouble(Density);
      var dict = new Rhino.Collections.ArchivableDictionary(1, "Physical");
      dict.Set("Weight", Weight);
      dict.Set("Density", Density);
      archive.WriteDictionary(dict);
      return true;
    }
  }


  [Guid("ca9a110e-3969-49ec-9d59-a7c2ee0b85bd")]
  public class ex_userdataCommand : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "cs_userdataCommand"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      Rhino.DocObjects.ObjRef objref;
      var rc = Rhino.Input.RhinoGet.GetOneObject("Select Object", false, Rhino.DocObjects.ObjectType.AnyObject, out objref);
      if (rc != Rhino.Commands.Result.Success)
        return rc;

      // See if user data of my custom type is attached to the geomtry
      var ud = objref.Geometry().UserData.Find(typeof(PhysicalData)) as PhysicalData;
      if (ud == null)
      {
        // No user data found; create one and add it
        int weight = 0;
        rc = Rhino.Input.RhinoGet.GetInteger("Weight", false, ref weight);
        if (rc != Rhino.Commands.Result.Success)
          return rc;

        ud = new PhysicalData(weight, 12.34);
        objref.Geometry().UserData.Add(ud);
      }
      else
      {
        RhinoApp.WriteLine("{0} = {1}", ud.Description, ud);
      }
      return Rhino.Commands.Result.Success;
    }
  }
}

