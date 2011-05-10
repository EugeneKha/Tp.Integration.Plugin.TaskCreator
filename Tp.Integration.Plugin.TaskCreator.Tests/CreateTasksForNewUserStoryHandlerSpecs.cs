// 
// Copyright (c) 2005-2011 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
// 
using System.Linq;
using NUnit.Framework;
using Tp.Integration.Common;
using Tp.Integration.Messages;
using Tp.Integration.Messages.EntityLifecycle;
using Tp.Integration.Messages.EntityLifecycle.Messages;
using Tp.Integration.Plugin.Common.Storage.Persisters;
using Tp.Integration.Testing.Common;
using Tp.Testing.Common.NUnit;

namespace Tp.Integration.Plugin.TaskCreator.Tests
{
	[TestFixture]
	public class CreateTasksForNewUserStoryHandlerSpecs
	{
		private TaskCreatorProfile _profileInstance;
		private TransportMock _transport;
		private UserStoryCreatedMessage _userStoryCreatedMessage;

		[SetUp]
		public void Init()
		{
			_transport = TransportMock.Create(typeof (CreateTasksForNewUserStoryHandler).Assembly);

			_profileInstance = CreateProfile("Profile#1");

			_userStoryCreatedMessage = new UserStoryCreatedMessage
			                           	{
			                           		Dto =
			                           			new UserStoryDTO
			                           				{
			                           					ProjectID = _profileInstance.Project,
			                           					UserStoryID = 123,
			                           					Name = "{CT}User Story Name"
			                           				}
			                           	};
		}


		[Test]
		public void ShouldCreateTask()
		{
			_transport.HandleMessageFromTp(_userStoryCreatedMessage);

			var taskDto = _transport.TpQueue.GetMessages<CreateCommand>().Last().Dto as TaskDTO;
			taskDto.UserStoryID.Should(Be.EqualTo(_userStoryCreatedMessage.Dto.UserStoryID));
			taskDto.Name.Should(Be.EqualTo(_profileInstance.TasksList));
		}

		#region Helpers

		private static TaskCreatorProfile CreateProfile(string profileName)
		{
			var plugin = TransportMock.Plugin;

			var account = new Account {Name = AccountName.Empty.Value};
			plugin.Accounts.Add(account);

			var profile = new Profile {Name = profileName, Initialized = true};
			account.Profiles.Add(profile);

			var profileInstance = profile.GetProfile<TaskCreatorProfile>();
			profileInstance.Project = 123;
			profileInstance.TasksList = "Task";
			profileInstance.CommandName = "{CT}";
			profile.SetProfile(profileInstance);
			return profileInstance;
		}

		#endregion
	}
}