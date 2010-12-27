﻿Partial Class Examples
  Public Shared Function AddTexture(ByVal doc As Rhino.RhinoDoc) As Rhino.Commands.Result
    ' Select object to add texture
    Dim filter As Rhino.DocObjects.ObjectType = Rhino.DocObjects.ObjectType.Surface Or Rhino.DocObjects.ObjectType.PolysrfFilter Or Rhino.DocObjects.ObjectType.Mesh
    Dim objref As Rhino.DocObjects.ObjRef = Nothing
    Dim rc As Rhino.Commands.Result = Rhino.Input.RhinoGet.GetOneObject("Select object to add texture", False, filter, objref)
    If rc <> Rhino.Commands.Result.Success Then
      Return rc
    End If

    Dim rhino_object As Rhino.DocObjects.RhinoObject = objref.[Object]()
    If rhino_object Is Nothing Then
      Return Rhino.Commands.Result.Failure
    End If

    ' Select texture
    Dim fd As New Rhino.UI.OpenFileDialog()
    fd.Filter = "Image Files (*.bmp;*.png;*.jpg)|*.bmp;*.png;*.jpg"
    If fd.ShowDialog() <> System.Windows.Forms.DialogResult.OK Then
      Return Rhino.Commands.Result.Cancel
    End If

    ' Verify texture
    Dim bitmap_filename As String = fd.FileName
    If String.IsNullOrEmpty(bitmap_filename) OrElse Not System.IO.File.Exists(bitmap_filename) Then
      Return Rhino.Commands.Result.[Nothing]
    End If

    ' Make sure the object has it's material source set to "material_from_object"
    If rhino_object.Attributes.MaterialSource <> Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject Then
      rhino_object.Attributes.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject
    End If

    ' Make sure the object has a material assigned
    Dim material_index As Integer = rhino_object.Attributes.MaterialIndex
    If material_index < 0 Then
      ' Create a new material based on Rhino's default material
      material_index = doc.Materials.AddMaterial()
      ' Assign the new material (index) to the object.
      rhino_object.Attributes.MaterialIndex = material_index
    End If

    If material_index >= 0 Then
      Dim mat As Rhino.DocObjects.Material = doc.Materials(material_index)
      mat.SetBitmapTexture(bitmap_filename)
      doc.Materials.Modify(mat, material_index, False)
      'Don't forget to update the object, if necessary
      rhino_object.CommitChanges()

      doc.Views.Redraw()
      Return Rhino.Commands.Result.Success
    End If

    Return Rhino.Commands.Result.Failure
  End Function
End Class
