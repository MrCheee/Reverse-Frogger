using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestFieldGrid
{
    [Test]
    public void get_single_grid_handling_out_of_bounds()
    {
        try
        {
            FieldGrid.GetSingleGrid(0, 0);
            FieldGrid.GetSingleGrid(12, 10);
            FieldGrid.GetSingleGrid(6, 5);
        }
        catch (Exception e)
        {
            Assert.Fail($"Expected No OOB Exception, but got: {e.Message}");
        }
        
        Assert.Throws<AccessingFieldGridOutOfBoundsException>(() => FieldGrid.GetSingleGrid(-1, 0));
        Assert.Throws<AccessingFieldGridOutOfBoundsException>(() => FieldGrid.GetSingleGrid(13, 13));
        Assert.Throws<AccessingFieldGridOutOfBoundsException>(() => FieldGrid.GetSingleGrid(0, 15));
    }

    [Test]
    // Field Grid is created as a 2-D array with [0, 0] being the bottommost, leftmost grid. 
    // As x increases, the grid moves rightwards. i.e. [1, 0] is bottommost, 2nd from left grid.
    // As y increases, the grid moves upwards. i.e. [0, 1] is leftmost, 2nd form bottom grid.
    public void is_field_grid_created_with_correct_numbering_and_coord()
    {
        // Arrange
        FieldGrid fgrid = FieldGrid.GetFieldGrid();

        // Act

        // Assert
        Assert.AreEqual(new Vector3(0, 0, 0), FieldGrid.GetSingleGrid(0, 0).GetGridCentrePoint());
        Assert.AreEqual(new Vector3(25, 0, 25), FieldGrid.GetSingleGrid(5, 5).GetGridCentrePoint());
        Assert.AreEqual(new Vector3(50, 0, 0), FieldGrid.GetSingleGrid(10, 0).GetGridCentrePoint());
        Assert.AreEqual(new Vector3(40, 0, 35), FieldGrid.GetSingleGrid(8, 7).GetGridCentrePoint());
        Assert.AreEqual(new GridCoord(0, 0), FieldGrid.GetSingleGrid(0, 0).GetGridCoord());
        Assert.AreEqual(new GridCoord(5, 5), FieldGrid.GetSingleGrid(5, 5).GetGridCoord());
        Assert.AreEqual(new GridCoord(8, 7), FieldGrid.GetSingleGrid(8, 7).GetGridCoord());
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator TestFieldGridWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
