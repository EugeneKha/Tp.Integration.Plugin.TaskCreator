﻿// 
// Copyright (c) 2005-2010 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
// 
using System.Linq;
using NBehave.Narrator.Framework;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap;
using Tp.Integration.Common;
using Tp.Integration.Messages.EntityLifecycle.Commands;
using Tp.Integration.Messages.EntityLifecycle.Messages;
using Tp.Integration.Plugin.Common;
using Tp.Integration.Plugin.Common.Storage;
using Tp.Testing.Common.NBehave;
using Tp.Testing.Common.NUnit;

namespace Tp.Integration.Plugin.TaskCreator.Tests
{
	[TestFixture]
	[ActionSteps]
	public class CreateTasksForNewUserStoryHandlerTests
	{
		private readonly UserStoryDTO _userStoryDto = new UserStoryDTO();

		[SetUp]
		public void Setup()
		{
			ObjectFactory.Initialize(x =>
			                         	{
			                         		x.For<IStorageRepository>().HybridHttpOrThreadLocalScoped().Use(
			                         			MockRepository.GenerateStub<IStorageRepository>());
			                         		x.For<CreateTasksForNewUserStoryHandler>().HybridHttpOrThreadLocalScoped().Use
			                         			<CreateTasksForNewUserStoryHandler>();
			                         		x.For<ITpBus>().HybridHttpOrThreadLocalScoped().Use<TpBusMock>();
			                         		x.Forward<ITpBus, TpBusMock>();
			                         		x.Forward<ITpBus, ICommandBus>();
			                         		x.Forward<ITpBus, ILocalBus>();
			                         	});
		}

		[Test]
		public void ShouldCreateTasksIfCommandFoundInTheBeginning()
		{
			@"
				Given test profile created
					And user story has id 100
					And user story has project 1
					And user story has name ' {CT}Test User Story'
					And user story has owner 2
				When UserStoryCreatedMessage received
				Then task 'Task 1' should be created for user story 100 with owner 2
					And task 'Task 2' should be created for user story 100 with owner 2
					And user story name should be updated to 'Test User Story'"
				.Execute();
		}

		[Test]
		public void ShouldCreateTasksOnUserStoryUpdate()
		{
			@"
				Given test profile created
					And user story has id 100
					And user story has project 1
					And user story has name ' {CT}Test User Story'
					And user story has owner 2
				When UserStoryUpdatedMessage received
				Then task 'Task 1' should be created for user story 100 with owner 2
					And task 'Task 2' should be created for user story 100 with owner 2
					And user story name should be updated to 'Test User Story'"
				.Execute();
		}

		[Test]
		public void ShouldNotCreateTasksIfCommandFoundInTheMiddle()
		{
			@"
				Given test profile created
					And user story has id 100
					And user story has project 1
					And user story has name 'Test User {CT}Story'
					And user story has owner 2
				When UserStoryCreatedMessage received
				Then no tasks should be created
					And user story name should not be updated"
				.Execute();
		}

		[Test]
		public void ShouldNotCreateTasksIfFilterDoesNotMatch()
		{
			@"
				Given test profile created
					And user story has id 100
					And user story has project 1
					And user story has name 'User Story'
					And user story has owner 2
				When UserStoryCreatedMessage received
				Then no tasks should be created
					And user story name should not be updated"
				.Execute();
		}

		[Test]
		public void ShouldNotCreateTasksIfProjectDoesNotMatch()
		{
			@"
				Given test profile created
					And user story has id 100
					And user story has project 1111111
					And user story has name 'Some User Story'
					And user story has owner 2
				When UserStoryCreatedMessage received
				Then no tasks should be created"
				.Execute();
		}

		[Test]
		public void ShouldNotCreateTasksForEmptyCommandName()
		{
			@"
				Given test profile created
					And test profile has empty command name
					And user story has id 100
					And user story has project 1
					And user story has name 'ComplexUS_1289'
					And user story has owner 2
				When UserStoryCreatedMessage received
				Then no tasks should be created"
				.Execute();
		}

		[Test]
		public void ShouldUpdateNameEvenIfTaskListIsEmpty()
		{
			@"
				Given test profile created
					And test profile has empty task list
					And user story has id 100
					And user story has project 1
					And user story has name '{CT}Test User Story'
					And user story has owner 2
				When UserStoryCreatedMessage received
				Then no tasks should be created
					And user story name should be updated to 'Test User Story'"
				.Execute();
		}

		[Given("test profile created")]
		public void CreateTestProfile()
		{
			ObjectFactory.GetInstance<IStorageRepository>().Stub(x => x.GetProfile<TaskCreatorProfile>()).Return(
				new TaskCreatorProfile
					{
						Project = 1,
						CommandName = "{CT}",
						TasksList =
							@"Task 1
Task 2"
					});
		}

		[Given("test profile has empty task list")]
		public void SetEmptyTaskList()
		{
			ObjectFactory.GetInstance<IStorageRepository>().GetProfile<TaskCreatorProfile>().TasksList = string.Empty;
		}

		[Given("user story has id $userStoryId")]
		public void SetUserStoryId(int userStoryId)
		{
			_userStoryDto.ID = userStoryId;
		}

		[Given("user story has project $projectId")]
		public void SetUserStoryProject(int projectId)
		{
			_userStoryDto.ProjectID = projectId;
		}

		[Given("user story has name '$userStoryName'")]
		public void SetUserStoryName(string userStoryName)
		{
			_userStoryDto.Name = userStoryName;
		}

		[Given("user story has owner $ownerId")]
		public void SetUserStoryOwner(int ownerId)
		{
			_userStoryDto.OwnerID = ownerId;
		}

		[Given("test profile has command name '$commandName'")]
		public void SetProfileCommandName(string commandName)
		{
			ObjectFactory.GetInstance<IStorageRepository>().GetProfile<TaskCreatorProfile>().CommandName =
				commandName;
		}

		[Given("test profile has empty command name")]
		public void SetEmptyCommandName()
		{
			SetProfileCommandName(null);
		}

		[When("UserStoryCreatedMessage received")]
		public void SendUserStoryCreatedMessage()
		{
			var createTasksForNewUserStoryHandler = ObjectFactory.GetInstance<CreateTasksForNewUserStoryHandler>();
			createTasksForNewUserStoryHandler.Handle(new UserStoryCreatedMessage {Dto = _userStoryDto});
		}

		[When("UserStoryUpdatedMessage received")]
		public void SendUserStoryUpdatedMessage()
		{
			var createTasksForNewUserStoryHandler = ObjectFactory.GetInstance<CreateTasksForNewUserStoryHandler>();
			createTasksForNewUserStoryHandler.Handle(new UserStoryUpdatedMessage
			                                         	{Dto = _userStoryDto, ChangedFields = new[] {UserStoryField.Name}});
		}

		[Then("task '$taskName' should be created for user story $userStoryId with owner $ownerId")]
		public void TaskShouldBeCreated(string taskName, int userStoryId, int ownerId)
		{
			ObjectFactory.GetInstance<TpBusMock>().CreateCommands.Any(x =>
			                                                          	{
			                                                          		if (!(x.Dto is TaskDTO)) return false;
			                                                          		var taskDto = x.Dto as TaskDTO;
			                                                          		return taskDto.Name == taskName &&
			                                                          		       taskDto.UserStoryID == userStoryId &&
			                                                          		       taskDto.OwnerID == ownerId;
			                                                          	}).Should(Be.True);
		}

		[Then("no tasks should be created")]
		public void NoTasksShouldBeCreated()
		{
			ObjectFactory.GetInstance<TpBusMock>().CreateCommands.Should(Be.Empty);
		}

		[Then("user story name should be updated to '$userStoryNameUpdated'")]
		public void UserStoryNameShouldBeUpdated(string userStoryNameUpdated)
		{
			ObjectFactory.GetInstance<TpBusMock>().UpdateCommandShouldMatch<UpdateUserStoryCommand>(
				x =>
				x.Dto.Name == userStoryNameUpdated && x.ChangedFields.Count() == 1 && x.ChangedFields[0].Equals(UserStoryField.Name));
		}

		[Then("user story name should not be updated")]
		public void UserStoryNameShouldBeUpdated()
		{
			ObjectFactory.GetInstance<TpBusMock>().UpdateCommands.Should(Be.Empty);
		}
	}
}