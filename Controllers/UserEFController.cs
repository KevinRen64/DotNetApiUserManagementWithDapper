using DotNetApi.Data;
using DotNetApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using DotNetApi.Models;
using AutoMapper;

namespace DotNetApi.Controllers;

[ApiController]  // Indicates this class defines a Web API controller
[Route("[controller]")]  // Route will be based on the controller name: /UserEF
public class UserEFController : ControllerBase
{

  private readonly IUserRepository _userRepository;  // Dependency for accessing user data from the repository
  private readonly IMapper _mapper;  // AutoMapper instance for mapping between DTOs and entities


  // Constructor initializes repository and configures AutoMapper
  public UserEFController(IConfiguration config, IUserRepository userRepository)
  {
    _userRepository = userRepository;

    // Configure AutoMapper to map from UserToAddDto to User entity
    _mapper = new Mapper(new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<UserToAddDto, User>();
    }));
  }


  // GET: /UserEF/GetUsers
  // Retrieves and returns all users from the database
  [HttpGet("GetUsers")]
  public IEnumerable<User> GetUsers()
  {
    IEnumerable<User> users = _userRepository.GetUsers();
    return users;
  }


  // GET: /UserEF/GetUsers/{userId}
  // Retrieves a single user by userId
  [HttpGet("GetUsers/{userId}")]
  public User GetSingleUser(int userId)
  {
    return _userRepository.GetSingleUser(userId);
  }


  // PUT: /UserEF/EditUser
  // Updates an existing user in the database
  [HttpPut("EditUser")]
  public IActionResult EditUser(User user)
  {
    // Retrieve existing user
    User? userDb = _userRepository.GetSingleUser(user.UserId);

    if (userDb != null)
    {
      // Update user fields
      userDb.Active = user.Active;
      userDb.FirstName = user.FirstName;
      userDb.LastName = user.LastName;
      userDb.Email = user.Email;
      userDb.Gender = user.Gender;

      // Save changes to the database
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }

      throw new Exception("Failed to Update User");
    }

    throw new Exception("Failed to Get User");

  }


  // POST: /UserEF/AddUser
  // Adds a new user to the database using a DTO
  [HttpPost("AddUser")]
  public IActionResult AddUser(UserToAddDto user)
  {

    // Convert DTO to User entity
    User userDb = _mapper.Map<User>(user);

    // Add to database and save
    _userRepository.AddEntity<User>(userDb);
    if (_userRepository.SaveChanges())
    {
      return Ok();
    }

    throw new Exception("Failed to Update User");
  }


  // DELETE: /UserEF/DeleteUser/{userId}
  // Deletes a user from the database by userId
  [HttpDelete("DeleteUser/{userId}")]
  public IActionResult DeleteUser(int userId)
  {
    // Find the user
    User? userDb = _userRepository.GetSingleUser(userId);

    if (userDb != null)
    {
      // Delete user and save changes
      _userRepository.RemoveEntity<User>(userDb);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }

      throw new Exception("Failed to Delete User");
    }
    throw new Exception("Failed to Get User");
  }


  // GET: /UserEF/GetAllUsersSalary
  // Returns salary information for all users
  [HttpGet("GetAllUsersSalary")]
  public IEnumerable<UserSalary> GetAllUsersSalary()
  {
    return _userRepository.GetUserSalaries();
  }


  // GET: /UserEF/GetUsersSalary/{userId}
  // Returns salary information for a specific user
  [HttpGet("GetUsersSalary/{userId}")]
  public UserSalary GetUsersSalary(int userId)
  {
    return _userRepository.GetSingleUserSalary(userId);
  }


  // POST: /UserEF/AddUserSalary
  // Adds salary info for a user
  [HttpPost("AddUserSalary")]
  public IActionResult AddUserSalary(UserSalary userSalaryInsert)
  {
    _userRepository.AddEntity<UserSalary>(userSalaryInsert);
    if (_userRepository.SaveChanges())
    {
      return Ok();
    }
    throw new Exception("Failed to Update User");
  }


  // PUT: /UserEF/EditUserSalary
  // Edits salary info for a user
  [HttpPut("EditUserSalary")]
  public IActionResult EditUserSalary(UserSalary userSalary)
  {
    // Find the user's salary record
    UserSalary? userSalaryDb = _userRepository.GetSingleUserSalary(userSalary.UserId);

    if (userSalaryDb != null)
    {
      // Update salary
      userSalaryDb.Salary = userSalary.Salary;

      // Save changes
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
      throw new Exception("Failed to Add User");
    }
    throw new Exception("Failed to Get User");
  }


  // DELETE: /UserEF/RemoveUserSalary
  // Removes a user's salary info
  [HttpDelete("RemoveUserSalary")]
  public IActionResult RemoveUserSalary(int userId)
  {
    // Find salary record
    UserSalary? userToDelete = _userRepository.GetSingleUserSalary(userId);

    if (userToDelete != null)
    {
      // Remove and save
      _userRepository.RemoveEntity<UserSalary>(userToDelete);
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
      throw new Exception("Failed to Add User");
    }
    throw new Exception("Failed to Get User");
  }


  // GET: /UserEF/GetUserJobInfos
  // Retrieves and returns all users from the database
  [HttpGet("GetUsersJobInfos")]
  public IEnumerable<UserJobInfo> GetUserJobInfos()
  {
    IEnumerable<UserJobInfo> userJobInfos = _userRepository.GetUserJobInfos();
    return userJobInfos;
  }


  // GET: /UserEF/GetUserJobInfos/{userId}
  // Returns job information for a specific user
  [HttpGet("GetUserJobInfos/{userId}")]
  public UserJobInfo GetUserJobInfo(int userId)
  {
    return _userRepository.GetSingleUserJobInfo(userId);
  }


  // PUT: /UserEF/EditUserJobInfo
  // Edits job info for a user
  [HttpPut("EditUserJobInfo")]
  public IActionResult EditUserJobInfo(UserJobInfo userJobToEdit)
  {
    // Find the user's salary record
    UserJobInfo? userJobInfoDb = _userRepository.GetSingleUserJobInfo(userJobToEdit.UserId);

    if (userJobInfoDb != null)
    {
      // Update salary
      userJobInfoDb.JobTitle = userJobToEdit.JobTitle;

      // Update department
      userJobInfoDb.Department = userJobToEdit.Department;

      // Save changes
      if (_userRepository.SaveChanges())
      {
        return Ok();
      }
      throw new Exception("Failed to Add User");
    }
    throw new Exception("Failed to Get User");
  }


  // POST: /UserEF/AddUserJobInfo
  // Adds job info for a user
  [HttpPost("AddUserJobInfo")]
  public IActionResult AddUserJobInfo(UserJobInfo userJobInfoToInsert)
  {
    _userRepository.AddEntity<UserJobInfo>(userJobInfoToInsert);
    if (_userRepository.SaveChanges())
    {
      return Ok();
    }
    throw new Exception("Failed to Update User");
  }
  

  // Delete: /UserEF/DeleteUserJobInfo
  // Adds job info for a user
  [HttpPost("DeleteUserJobInfo/{userId}")]
  public IActionResult DeleteUserJobInfo(int userId)
  {
    // Find the user's salary record
    UserJobInfo userJobIntoToDelete = _userRepository.GetSingleUserJobInfo(userId);

    // Remove and save
    _userRepository.RemoveEntity(userJobIntoToDelete);
    if (_userRepository.SaveChanges())
    {
      return Ok();
    }
    throw new Exception("Failed to Update User");
  }
}