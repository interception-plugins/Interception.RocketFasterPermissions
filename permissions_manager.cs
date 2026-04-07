using System;
using System.Collections.Generic;

using Steamworks;

using Rocket.API.Serialisation;
using Rocket.Core.Assets;

using interception.libraries.rocketfasterpermissions.types;
using SDG.Unturned;
using interception.extensions;
using Rocket.API;

namespace interception.libraries.rocketfasterpermissions {
    // todo more methods
    // todo make priority work

    public delegate void on_before_player_added_to_group_callback(string group_id, IRocketPlayer player, ref bool allow, ref RocketPermissionsProviderResult override_result);
    public delegate void on_after_player_added_to_group_callback(string group_id, IRocketPlayer player, RocketPermissionsProviderResult result);

    public delegate void on_before_player_removed_from_group_callback(string group_id, IRocketPlayer player, ref bool allow, ref RocketPermissionsProviderResult override_result);
    public delegate void on_after_player_removed_from_group_callback(string group_id, IRocketPlayer player, RocketPermissionsProviderResult result);

    public delegate void on_permissions_reloaded_callback();

    public static class permissions_manager {
        public static on_before_player_added_to_group_callback on_before_player_added_to_group_global;
        public static on_after_player_added_to_group_callback on_after_player_added_to_group_global;

        public static on_before_player_removed_from_group_callback on_before_player_removed_from_group_global;
        public static on_after_player_removed_from_group_callback on_after_player_removed_from_group_global;

        public static on_permissions_reloaded_callback on_permissions_reloaded_global;

        static Asset<RocketPermissions> permissions;
        // key is rocket group's id
        static Dictionary<string, rocket_group> groups;
        static Dictionary<ulong, HashSet<string>> players_permissions;

        static HashSet<string> perm_list_to_hashset(List<Permission> l) {
            HashSet<string> hs = new HashSet<string>();
            if (l == null)
                return hs;
            var len = l.Count;
            for (int i = 0; i < len; i++) {
                if (hs.Contains(l[i].Name)) continue;
                hs.Add(l[i].Name);
            }
            return hs;
        }

        static HashSet<ulong> member_list_to_hashset(List<string> l) {
            HashSet<ulong> hs = new HashSet<ulong>();
            if (l == null)
                return hs;
            var len = l.Count;
            for (int i = 0; i < len; i++) {
                ulong sid;
                if (!ulong.TryParse(l[i], out sid) || hs.Contains(sid)) continue;
                hs.Add(sid);
            }
            return hs;
        }

        static void get_parent_groups(string root_id, Dictionary<string, rocket_group> dict) {
            if (!groups.ContainsKey(root_id)) return;
            if (!dict.ContainsKey(root_id)) {
                dict.Add(root_id, groups[root_id]);
                if (groups[root_id].parent_group != null)
                    get_parent_groups(groups[root_id].parent_group, dict);
            }
        }

        internal static void trigger_on_before_player_added_to_group_global(string group_id, IRocketPlayer player, ref bool allow, ref RocketPermissionsProviderResult override_result) {
            if (on_before_player_added_to_group_global != null)
                on_before_player_added_to_group_global(group_id, player, ref allow, ref override_result);
        }

        internal static void trigger_on_after_player_added_to_group_global(string group_id, IRocketPlayer player, RocketPermissionsProviderResult result) {
            if (on_after_player_added_to_group_global != null)
                on_after_player_added_to_group_global(group_id, player, result);
        }

        internal static void trigger_on_before_player_removed_from_group_global(string group_id, IRocketPlayer player, ref bool allow, ref RocketPermissionsProviderResult override_result) {
            if (on_before_player_removed_from_group_global != null)
                on_before_player_removed_from_group_global(group_id, player, ref allow, ref override_result);
        }

        internal static void trigger_on_after_player_removed_from_group_global(string group_id, IRocketPlayer player, RocketPermissionsProviderResult result) {
            if (on_after_player_removed_from_group_global != null)
                on_after_player_removed_from_group_global(group_id, player, result);
        }

        internal static void trigger_on_permissions_reloaded_global() {
            if (on_permissions_reloaded_global != null)
                on_permissions_reloaded_global();
        }

        internal static void reload_groups(bool idk = false) {
            // no web permissions support as literally NO ONE uses them
            permissions = new XMLFileAsset<RocketPermissions>(Rocket.Core.Environment.PermissionFile);
            groups.Clear();
            var len = permissions.Instance.Groups.Count;
            for (int i = 0; i < len; i++) {
                groups.Add(permissions.Instance.Groups[i].Id, new rocket_group() {
                    //display_name = permissions.Instance.Groups[i].DisplayName ?? string.Empty,
                    //prefix = permissions.Instance.Groups[i].Prefix ?? string.Empty,
                    //suffix = permissions.Instance.Groups[i].Suffix ?? string.Empty,
                    //color = permissions.Instance.Groups[i].Color ?? string.Empty,
                    members = member_list_to_hashset(permissions.Instance.Groups[i].Members),
                    permissions = perm_list_to_hashset(permissions.Instance.Groups[i].Permissions),
                    parent_group = permissions.Instance.Groups[i].ParentGroup/* ?? string.Empty*/,
                    //priority = permissions.Instance.Groups[i].Priority
                });
            }
            if (idk && groups.ContainsKey(permissions.Instance.DefaultGroup)) {
                var len2 = Provider.clients.Count;
                for (int i = 0; i < len2; i++) {
                    var sid = Provider.clients[i].get_steamid64();
                    if (groups[permissions.Instance.DefaultGroup].members.Contains(sid)) continue;
                    groups[permissions.Instance.DefaultGroup].members.Add(sid);
                }
            }
        }

        internal static void reload_player_permissions(ulong sid) {
            if (!players_permissions.ContainsKey(sid)) return; // todo throw exception here
            players_permissions[sid].Clear();
            foreach (var group in groups.Values) {
                if (!group.members.Contains(sid)) continue;
                foreach (var permission in group.permissions) {
                    if (players_permissions[sid].Contains(permission)) continue;
                    players_permissions[sid].Add(permission);
                }
            }
        }

        internal static void reload_players_permissions() {
            players_permissions.Clear();
            var len = Provider.clients.Count;
            for (int i = 0; i < len; i++) {
                var sid = Provider.clients[i].get_steamid64();
                reload_player_permissions(sid);
            }
        }

        internal static void reload() {
            reload_groups(true);
            reload_players_permissions();
        }

        internal static void add_player_to_group(string group_id, ulong sid) {
            if (!groups.ContainsKey(group_id) || groups[group_id].members.Contains(sid)) return; // todo throw exception here maybe
            groups[group_id].members.Add(sid);
            reload_player_permissions(sid);
        }

        internal static void remove_player_from_group(string group_id, ulong sid) {
            if (!groups.ContainsKey(group_id) || !groups[group_id].members.Contains(sid)) return; // todo throw exception here maybe
            groups[group_id].members.Remove(sid);
            reload_player_permissions(sid);
        }

        public static bool has_permission(ulong sid, string permission) {
            return players_permissions.ContainsKey(sid) && players_permissions[sid].Contains(permission);
        }

        public static bool has_permission(CSteamID csid, string permission) {
            return players_permissions.ContainsKey(csid.m_SteamID) && players_permissions[csid.m_SteamID].Contains(permission);
        }

        public static bool has_permission(Player player, string permission, bool ignore_admin = false) {
            if (!ignore_admin && player.channel.owner.isAdmin) return true;
            var sid = player.get_steamid64();
            return players_permissions.ContainsKey(sid) && players_permissions[sid].Contains(permission);
        }

        public static bool has_permission(IRocketPlayer rocket_player, string permission, bool ignore_admin = false) {
            if (rocket_player is ConsolePlayer || (!ignore_admin && rocket_player.IsAdmin)) return true;
            ulong sid = ulong.Parse(rocket_player.Id);// WHY TF ROCKET IS STORING STEAM ID AS A STRING
            return players_permissions.ContainsKey(sid) && players_permissions[sid].Contains(permission);
        }

        public static bool is_player_in_group(ulong sid, string group_id, bool include_parent_groups) {
            if (group_id == permissions.Instance.DefaultGroup) return true;

            if (!include_parent_groups)
                return groups.ContainsKey(group_id) && groups[group_id].members.Contains(sid);

            Dictionary<string, rocket_group> parent_groups = new Dictionary<string, rocket_group>();
            get_parent_groups(group_id, parent_groups);
            foreach (var group in parent_groups) {
                if (group.Value.members.Contains(sid)) return true;
            }
            return false;
        }

        public static bool is_player_in_group(CSteamID csid, string group_id, bool include_parent_groups) {
            return is_player_in_group(csid.m_SteamID, group_id, include_parent_groups);
        }

        public static bool is_player_in_group(Player player, string group_id, bool include_parent_groups) {
            return is_player_in_group(player.get_steamid64(), group_id, include_parent_groups);
        }

        public static bool is_player_in_group(IRocketPlayer rocket_player, string group_id, bool include_parent_groups) {
            return is_player_in_group(ulong.Parse(rocket_player.Id), group_id, include_parent_groups);
        }

        static void on_server_connected(CSteamID csid) {
            if (PlayerTool.getPlayer(csid) == null) return; // not sure if its necessary
            var sid = csid.m_SteamID;
            if (groups.ContainsKey(permissions.Instance.DefaultGroup) && !groups[permissions.Instance.DefaultGroup].members.Contains(sid)) {
                groups[permissions.Instance.DefaultGroup].members.Add(sid);
            }
            if (!players_permissions.ContainsKey(sid)) {
                players_permissions.Add(sid, new HashSet<string>());
                reload_player_permissions(sid);
            }
        }

        static void on_server_disconnected(CSteamID csid) {
            if (PlayerTool.getPlayer(csid) == null) return; // not sure if its necessary
            var sid = csid.m_SteamID;
            if (players_permissions.ContainsKey(sid))
                players_permissions.Remove(sid);
        }

        internal static void init() {
            groups = new Dictionary<string, rocket_group>();
            reload_groups();
            players_permissions = new Dictionary<ulong, HashSet<string>>();
            reload_players_permissions();
            Provider.onServerConnected += on_server_connected;
            Provider.onServerDisconnected += on_server_disconnected;
        }

        internal static void uninit() {
            Provider.onServerConnected -= on_server_connected;
            Provider.onServerDisconnected -= on_server_disconnected;
            groups.Clear();
            groups = null;
            players_permissions.Clear();
            players_permissions = null;
        }
    }
}
