// 
// Copyright (c) 2005-2010 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
// 
using System;
using Tp.Integration.Messages.ComponentModel;
using Tp.Integration.Plugin.Common;

namespace Tp.Integration.Plugin.TaskCreator
{
	[Profile, Serializable]
	public class TaskCreatorProfile
	{
		public TaskCreatorProfile()
		{
			Project = int.MinValue;
			TasksList = string.Empty;
		}

		[ProjectId]
		[TPDescription("Select project (the plugin will work for user stories from this project)")]
		[TpCategotyAttribute("Task Creator Settings")]
		public int Project { get; set; }

		private string _commandName = "{Type your command here}";

		[TPDescription(@"As you add/edit a user story, you should start user story name with special command.
e.g. we've chosen {CT} as a command in profile.<br/>
<em>User Guide</em> is the name of this user story. So, if we put <em>{CT}User Guide</em> to user story name field, 
a set of tasks specified below in 'Tasks List' field will be created<br/> and command <em>{CT}</em> will be cut from user story name.")]
		[TpCategotyAttribute("Task Creator Settings")]
		public string CommandName
		{
			get { return _commandName; }
			set { _commandName = value; }
		}

		[TextArea]
		[TPDescription("List of tasks to be created. One task per line.")]
		[TpCategotyAttribute("Task Creator Settings")]
		public string TasksList { get; set; }
	}
}