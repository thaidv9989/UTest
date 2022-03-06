using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using D7_ASPMVC.Interfaces;
using D7_ASPMVC.Implement;
using D7_ASPMVC.Controllers;
using D7_ASPMVC.Models;
using System;

namespace D7.Tests;

public class PeopleControllerUTest
{
    private Mock<IPerson> _personMock;
    private Mock<ILogger<PeopleController>> _loggerMock;
    
    static List<PersonModel> _people = new List<PersonModel>
    {
        new PersonModel{
                ID = 1,
                FirstName = "Thai",
                LastName = "Do Van",
                Gender = "Male",
                DOB = new DateTime(2001, 2, 15),
                PhoneNumber = "0989479615",
                Address = "Thai Binh"
            },
            new PersonModel{
                ID = 2,
                FirstName = "Hoc",
                LastName = "Nguyen Thai",
                Gender = "Male",
                DOB = new DateTime(2000, 2, 15),
                PhoneNumber = "0989479615",
                Address = "Ha Nam"
            },
            new PersonModel{
                ID = 3,
                FirstName = "Thanh",
                LastName = "Do Tien",
                Gender = "Male",
                DOB = new DateTime(1999, 2, 15),
                PhoneNumber = "0989479615",
                Address = "Ha Noi"
            }, 
    };

    [SetUp]
    public void Setup()
    {
        _personMock = new Mock<IPerson>();
        _loggerMock = new Mock<ILogger<PeopleController>>();
        _personMock.Setup(x => x.GetAll()).Returns(_people);
    }
    [Test]
    public void Index_ReturnsViewResult_WithAllListOfPerson()
    {
        //Arrange    
        var controller = new PeopleController(_loggerMock.Object, _personMock.Object);
        var expectedCount = _people.Count;
        //Act
        var rs = controller.Index();
        //assert
        Assert.IsInstanceOf<ViewResult>(rs, "Invalid return type");

        var view = (ViewResult)rs;
        Assert.IsAssignableFrom<List<PersonModel>?>(view.ViewData.Model, "Invalid viewdata model");

        var model = view.ViewData.Model as List<PersonModel>;
        Assert.IsNotNull(model, "Data model must not be null!!!");
        Assert.AreEqual(expectedCount, model?.Count, "Model count is not equal to expected count");    
    }
    [Test]
    public void Create_IsvalidModel_ReturnViews_WithErrorModelState()
    {
        const string key = "ERROR";
        const string message = "Invalid model";
        //arrange
        var controller = new PeopleController(_loggerMock.Object, _personMock.Object);
        controller.ModelState.AddModelError(key, message);

        //act
        var rs = controller.Add(null);
        //assert
        Assert.IsInstanceOf<ViewResult>(rs, "Invalid return type");
        var view = (ViewResult)rs;
        var modelState = view.ViewData.ModelState;

        Assert.IsFalse(modelState.IsValid, "Invalid model state");
        Assert.AreEqual(1, modelState.ErrorCount, "");

        modelState.TryGetValue(key, out var value);
        var error = value?.Errors.First().ErrorMessage;
        Assert.AreEqual(message, error);
    }
    [Test]
    public void Create_ValidModel_ReturnRedirectToActIndex()
    {
        var person = new PersonModel
        {
            ID = 4,
            FirstName = "Mang",
            LastName = "Nguyen Ba",
            DOB = new DateTime(2001, 2, 15),
            Address = "Ha Noi"
        };
        _personMock.Setup(x => x.Add(person))
        .Callback<PersonModel>((PersonModel p) =>
        {
            _people.Add(p);
        });

        //arrange
        var controller = new PeopleController(_loggerMock.Object, _personMock.Object);
        var expected = _people.Count + 1;


        //act
        var rs = controller.Add(person);
        //assert
        Assert.IsInstanceOf<RedirectToActionResult>(rs, "Invalid return type");
        var view = (RedirectToActionResult)rs;

        Assert.AreEqual("Index", view.ActionName, "Invalid action name...");

        var actual = _people.Count;

        Assert.AreEqual(expected, actual, "Error");

        Assert.AreEqual(person, _people.Last(), "Not Item");
    }
    [Test]
    public void Edit_ReturnPeopleWithEdited()
    {
        var person = new PersonModel
        {
            ID = 4,
            FirstName = "Mang",
            LastName = "Nguyen Ba",
            DOB = new DateTime(2001, 2, 15),
            Address = "Ha Noi"
        };
        _personMock.Setup(p => p.Edit(person)).Callback<PersonModel>((PersonModel model) => _people[0] = model);

        //Arrange
        var expectedName = person.LastName;
        var expectedCount = _people.Count;
        var controller = new PeopleController(_loggerMock.Object, _personMock.Object);

        //Act
        var result = controller.Edit(person);
        var actualName = _people[0].LastName;
        var actualCount = _people.Count;

        //Assert
        Assert.IsInstanceOf<RedirectToActionResult>(result, "Edit action should return a RedirectToActionResult");
        Assert.IsNotNull(result, "Edit action should not null");
        var view = (RedirectToActionResult)result;
        Assert.AreEqual("Index", view.ActionName, "Edit action should return a RedirectToActionResult to Index action");
        Assert.AreEqual(expectedName, actualName, "Edit action should update the person's last name");
        Assert.AreEqual(expectedCount, actualCount, "Edit action should not add a new person");

    }
    [Test]
    public void Delete_ReturnViewListPeople_Deleted()
    {
        var person = new PersonModel
        {
            ID = 4,
            FirstName = "Mang",
            LastName = "Nguyen Ba",
            DOB = new DateTime(2001, 2, 15),
            Address = "Ha Noi"
        };
        // Arrange
        _personMock.Setup(x => x.Detail(1)).Returns(person);
        var controller = new PeopleController(_loggerMock.Object, _personMock.Object);
        // Act
        controller.Delete(1);
        // Assert   
        _personMock.Verify(r => r.Delete(1));
    }
    [TearDown]
    public void TearDown()
    {
        /*_personMock = null;*/ 
    }
}