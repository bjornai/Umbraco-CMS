﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using UserProfile = Umbraco.Web.Models.ContentEditing.UserProfile;

namespace Umbraco.Web.Models.Mapping
{
    internal class UserModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            
            config.CreateMap<UserGroupSave, IUserGroup>()
                .ConstructUsing((UserGroupSave save) => new UserGroup() { CreateDate = DateTime.Now })
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(dest => dest.Id, map => map.Condition(source => GetIntId(source.Id) > 0))
                .ForMember(dest => dest.Id, map => map.MapFrom(source => GetIntId(source.Id)))
                .AfterMap((save, userGroup) =>
                {
                    userGroup.ClearAllowedSections();
                    foreach (var section in save.Sections)
                    {
                        userGroup.AddAllowedSection(section);
                    }
                });

            //Used for merging existing UserSave to an existing IUser instance - this will not create an IUser instance!
            config.CreateMap<UserSave, IUser>()
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(dest => dest.Id, map => map.Condition(source => GetIntId(source.Id) > 0))
                .ForMember(detail => detail.SessionTimeout, opt => opt.Ignore())
                .ForMember(detail => detail.EmailConfirmedDate, opt => opt.Ignore())
                .ForMember(detail => detail.InvitedDate, opt => opt.Ignore())
                .ForMember(detail => detail.SecurityStamp, opt => opt.Ignore())
                .ForMember(detail => detail.Avatar, opt => opt.Ignore())
                .ForMember(detail => detail.ProviderUserKey, opt => opt.Ignore())
                .ForMember(detail => detail.RawPasswordValue, opt => opt.Ignore())
                .ForMember(detail => detail.RawPasswordAnswerValue, opt => opt.Ignore())
                .ForMember(detail => detail.PasswordQuestion, opt => opt.Ignore())
                .ForMember(detail => detail.Comments, opt => opt.Ignore())
                .ForMember(detail => detail.IsApproved, opt => opt.Ignore())
                .ForMember(detail => detail.IsLockedOut, opt => opt.Ignore())
                .ForMember(detail => detail.LastLoginDate, opt => opt.Ignore())
                .ForMember(detail => detail.LastPasswordChangeDate, opt => opt.Ignore())
                .ForMember(detail => detail.LastLockoutDate, opt => opt.Ignore())
                .ForMember(detail => detail.FailedPasswordAttempts, opt => opt.Ignore())                
                .ForMember(user => user.Language, expression => expression.MapFrom(save => save.Culture))                
                .AfterMap((save, user) =>
                {
                    user.ClearGroups();
                    var foundGroups = applicationContext.Services.UserService.GetUserGroupsByAlias(save.UserGroups.ToArray());
                    foreach (var group in foundGroups)
                    {
                        user.AddGroup(group.ToReadOnlyGroup());
                    }
                });

            config.CreateMap<UserInvite, IUser>()
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(detail => detail.Id, opt => opt.Ignore())
                .ForMember(detail => detail.StartContentIds, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaIds, opt => opt.Ignore())
                .ForMember(detail => detail.Language, opt => opt.Ignore())
                .ForMember(detail => detail.Username, opt => opt.Ignore())
                .ForMember(detail => detail.PasswordQuestion, opt => opt.Ignore())
                .ForMember(detail => detail.SessionTimeout, opt => opt.Ignore())
                .ForMember(detail => detail.EmailConfirmedDate, opt => opt.Ignore())
                .ForMember(detail => detail.InvitedDate, opt => opt.Ignore())
                .ForMember(detail => detail.SecurityStamp, opt => opt.Ignore())
                .ForMember(detail => detail.Avatar, opt => opt.Ignore())
                .ForMember(detail => detail.ProviderUserKey, opt => opt.Ignore())
                .ForMember(detail => detail.RawPasswordValue, opt => opt.Ignore())
                .ForMember(detail => detail.RawPasswordAnswerValue, opt => opt.Ignore())
                .ForMember(detail => detail.Comments, opt => opt.Ignore())
                .ForMember(detail => detail.IsApproved, opt => opt.Ignore())
                .ForMember(detail => detail.IsLockedOut, opt => opt.Ignore())
                .ForMember(detail => detail.LastLoginDate, opt => opt.Ignore())
                .ForMember(detail => detail.LastPasswordChangeDate, opt => opt.Ignore())
                .ForMember(detail => detail.LastLockoutDate, opt => opt.Ignore())
                .ForMember(detail => detail.FailedPasswordAttempts, opt => opt.Ignore())                
                //all invited users will not be approved, completing the invite will approve the user
                .ForMember(user => user.IsApproved, expression => expression.UseValue(false))                
                .AfterMap((invite, user) =>
                {
                    user.ClearGroups();
                    var foundGroups = applicationContext.Services.UserService.GetUserGroupsByAlias(invite.UserGroups.ToArray());
                    foreach (var group in foundGroups)
                    {
                        user.AddGroup(group.ToReadOnlyGroup());
                    }
                });

            config.CreateMap<IReadOnlyUserGroup, UserGroupBasic>()
                .ForMember(detail => detail.StartContentId, opt => opt.Ignore())
                .ForMember(detail => detail.UserCount, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaId, opt => opt.Ignore())
                .ForMember(detail => detail.Key, opt => opt.Ignore())
                .ForMember(detail => detail.Sections, opt => opt.Ignore())
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(applicationContext.Services, group, display);
                });

            config.CreateMap<IUserGroup, UserGroupBasic>()
                .ForMember(detail => detail.StartContentId, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaId, opt => opt.Ignore())
                .ForMember(detail => detail.Sections, opt => opt.Ignore())
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(applicationContext.Services, group, display);
                });

            //create a map to assign a user group's default permissions to the AssignedUserGroupPermissions instance
            config.CreateMap<IUserGroup, AssignedUserGroupPermissions>()
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .ForMember(detail => detail.Id, opt => opt.MapFrom(group => group.Id))
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(detail => detail.AssignedPermissions, expression => expression.ResolveUsing(new PermissionsResolver(applicationContext.Services.TextService)))
                .AfterMap((group, display) =>
                {
                    if (display.Icon.IsNullOrWhiteSpace())
                    {
                        display.Icon = "icon-users";
                    }
                });
            config.CreateMap<IUserGroup, UserGroupDisplay>()
                .ForMember(detail => detail.StartContentId, opt => opt.Ignore())
                .ForMember(detail => detail.StartMediaId, opt => opt.Ignore())
                .ForMember(detail => detail.Sections, opt => opt.Ignore())
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(userGroup => "-1," + userGroup.Id))
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .ForMember(detail => detail.Users, opt => opt.Ignore())
                .ForMember(detail => detail.DefaultPermissions, expression => expression.ResolveUsing(new PermissionsResolver(applicationContext.Services.TextService)))
                .AfterMap((group, display) =>
                {
                    MapUserGroupBasic(applicationContext.Services, group, display);

                    //Important! Currently we are never mapping to multiple UserGroupDisplay objects but if we start doing that
                    // this will cause an N+1 and we'll need to change how this works.
                    var users = applicationContext.Services.UserService.GetAllInGroup(group.Id);
                    display.Users = Mapper.Map<IEnumerable<UserBasic>>(users);
                });

            config.CreateMap<IUser, UserDisplay>()
                .ForMember(detail => detail.Avatars, opt => opt.MapFrom(user => user.GetCurrentUserAvatarUrls(applicationContext.Services.UserService, applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(detail => detail.LastLoginDate, opt => opt.MapFrom(user => user.LastLoginDate == default(DateTime) ? null : (DateTime?) user.LastLoginDate))
                .ForMember(detail => detail.UserGroups, opt => opt.MapFrom(user => user.Groups))
                .ForMember(detail => detail.StartContentIds, opt => opt.UseValue(Enumerable.Empty<EntityBasic>()))
                .ForMember(detail => detail.StartMediaIds, opt => opt.UseValue(Enumerable.Empty<EntityBasic>()))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))                
                .ForMember(
                    detail => detail.AvailableCultures,
                    opt => opt.MapFrom(user => applicationContext.Services.TextService.GetSupportedCultures().ToDictionary(x => x.Name, x => x.DisplayName)))
                .ForMember(
                    detail => detail.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().GenerateHash()))
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(user => "-1," + user.Id))
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Icon, opt => opt.Ignore())
                .ForMember(detail => detail.IsCurrentUser, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.ResetPasswordValue, opt => opt.Ignore())
                .ForMember(detail => detail.Alias, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore())
                .AfterMap((user, display) =>
                {
                    //Important! Currently we are never mapping to multiple UserDisplay objects but if we start doing that
                    // this will cause an N+1 and we'll need to change how this works.

                    var startContentIds = user.StartContentIds.ToArray();
                    if (startContentIds.Length > 0)
                    {
                        //TODO: Update GetAll to be able to pass in a parameter like on the normal Get to NOT load in the entire object!

                        var contentItems = applicationContext.Services.EntityService.GetAll(UmbracoObjectTypes.Document, startContentIds);
                        display.StartContentIds = Mapper.Map<IEnumerable<IUmbracoEntity>, IEnumerable<EntityBasic>>(contentItems);
                    }
                    var startMediaIds = user.StartContentIds.ToArray();
                    if (startMediaIds.Length > 0)
                    {
                        var mediaItems = applicationContext.Services.EntityService.GetAll(UmbracoObjectTypes.Document, startMediaIds);
                        display.StartMediaIds = Mapper.Map<IEnumerable<IUmbracoEntity>, IEnumerable<EntityBasic>>(mediaItems);
                    }
                });

            config.CreateMap<IUser, UserBasic>()
                //Loading in the user avatar's requires an external request if they don't have a local file avatar, this means that initial load of paging may incur a cost
                //Alternatively, if this is annoying the back office UI would need to be updated to request the avatars for the list of users separately so it doesn't look
                //like the load time is waiting.
                .ForMember(detail => 
                    detail.Avatars, 
                    opt => opt.MapFrom(user => user.GetCurrentUserAvatarUrls(applicationContext.Services.UserService, applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(detail => detail.UserGroups, opt => opt.MapFrom(user => user.Groups))
                .ForMember(detail => detail.LastLoginDate, opt => opt.MapFrom(user => user.LastLoginDate == default(DateTime) ? null : (DateTime?) user.LastLoginDate))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(
                    detail => detail.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().ToMd5()))
                .ForMember(detail => detail.ParentId, opt => opt.UseValue(-1))
                .ForMember(detail => detail.Path, opt => opt.MapFrom(user => "-1," + user.Id))
                .ForMember(detail => detail.Notifications, opt => opt.Ignore())
                .ForMember(detail => detail.IsCurrentUser, opt => opt.Ignore())
                .ForMember(detail => detail.Udi, opt => opt.Ignore())
                .ForMember(detail => detail.Icon, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.Alias, opt => opt.Ignore())
                .ForMember(detail => detail.Trashed, opt => opt.Ignore())
                .ForMember(detail => detail.AdditionalData, opt => opt.Ignore());

            config.CreateMap<IUser, UserDetail>()
                .ForMember(detail => detail.Avatars, opt => opt.MapFrom(user => user.GetCurrentUserAvatarUrls(applicationContext.Services.UserService, applicationContext.ApplicationCache.RuntimeCache)))
                .ForMember(detail => detail.UserId, opt => opt.MapFrom(user => GetIntId(user.Id)))
                .ForMember(detail => detail.StartContentIds, opt => opt.MapFrom(user => user.StartContentIds))
                .ForMember(detail => detail.StartMediaIds, opt => opt.MapFrom(user => user.StartMediaIds))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(
                    detail => detail.EmailHash,
                    opt => opt.MapFrom(user => user.Email.ToLowerInvariant().Trim().GenerateHash()))
                .ForMember(detail => detail.SecondsUntilTimeout, opt => opt.Ignore());            

            config.CreateMap<IProfile, UserProfile>()
                  .ForMember(detail => detail.UserId, opt => opt.MapFrom(profile => GetIntId(profile.Id)));

            config.CreateMap<IUser, UserData>()
                .ConstructUsing((IUser user) => new UserData())
                .ForMember(detail => detail.Id, opt => opt.MapFrom(user => user.Id))
                .ForMember(detail => detail.AllowedApplications, opt => opt.MapFrom(user => user.AllowedSections))
                .ForMember(detail => detail.RealName, opt => opt.MapFrom(user => user.Name))
                .ForMember(detail => detail.Roles, opt => opt.MapFrom(user => user.Groups.ToArray()))
                .ForMember(detail => detail.StartContentNodes, opt => opt.MapFrom(user => user.StartContentIds))
                .ForMember(detail => detail.StartMediaNodes, opt => opt.MapFrom(user => user.StartMediaIds))
                .ForMember(detail => detail.Username, opt => opt.MapFrom(user => user.Username))
                .ForMember(detail => detail.Culture, opt => opt.MapFrom(user => user.GetUserCulture(applicationContext.Services.TextService)))
                .ForMember(detail => detail.SessionId, opt => opt.MapFrom(user => user.SecurityStamp.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString("N") : user.SecurityStamp));
            
        }

        private void MapUserGroupBasic(ServiceContext services, dynamic group, UserGroupBasic display)
        {
            var allSections = services.SectionService.GetSections();
            display.Sections = allSections.Where(x => Enumerable.Contains(group.AllowedSections, x.Alias)).Select(Mapper.Map<ContentEditing.Section>);
            if (group.StartMediaId > 0)
            {
                display.StartMediaId = Mapper.Map<EntityBasic>(
                    services.EntityService.Get(group.StartMediaId, UmbracoObjectTypes.Media));
            }
            if (group.StartContentId > 0)
            {
                display.StartContentId = Mapper.Map<EntityBasic>(
                    services.EntityService.Get(group.StartContentId, UmbracoObjectTypes.Document));
            }
            if (display.Icon.IsNullOrWhiteSpace())
            {
                display.Icon = "icon-users";
            }
        }

        private static int GetIntId(object id)
        {
            var result = id.TryConvertTo<int>();
            if (result.Success == false)
            {
                throw new InvalidOperationException(
                    "Cannot convert the profile to a " + typeof(UserDetail).Name + " object since the id is not an integer");
            }
            return result.Result;
        } 
 
    }
}