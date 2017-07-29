using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWServerManager
{
    public class ServerProperty
    {
        public string admins { get; set; }

        public string contact { get; set; }

        public int database_mysql_connections { get; set; }

        public string database_mysql_database { get; set; }

        public string database_mysql_password { get; set; }

        public string database_mysql_server_ip { get; set; }
        public int database_mysql_server_port { get; set; }
        public string database_mysql_user { get; set; }
        public string database_type { get; set; }
        public bool plugins_enabled { get; set; }
        public int plugins_error_threshold { get; set; }
        public string plugins_jdk_path { get; set; }
        public bool rcon_enabled { get; set; }
        public string rcon_password { get; set; }
        public int rcon_port { get; set; }
        public string server_custom_logo { get; set; }
        public bool server_hive_verification { get; set; }
        public string server_http_ip { get; set; }
        public string server_ip { get; set; }
        public bool server_list_visible { get; set; }
        public int server_memory { get; set; }
        public string server_name { get; set; }
        public string server_password { get; set; }
        public int server_port { get; set; }
        public string server_screen_title { get; set; }
        public string server_world_disabled_dungeons { get; set; }
        public string server_world_disabled_npcs { get; set; }
        public string server_world_disabled_watersources { get; set; }
        public bool server_world_generatecaves { get; set; }
        public bool server_world_generatevegetations { get; set; }
        public string server_world_name { get; set; }
        public int server_world_oreamount { get; set; }
        public string server_world_seed { get; set; }
        public string server_world_type { get; set; }
        public bool settings_admins_allpermissions { get; set; }
        public bool settings_animals_enabled { get; set; }
        public bool settings_blacklisted { get; set; }
        public bool settings_check_version { get; set; }
        public bool settings_chests_drop_items { get; set; }
        public bool settings_create_serverlog { get; set; }
        public bool settings_create_worldbackup { get; set; }
        public int settings_deadnpc_despawntime { get; set; }
        public bool settings_deadplayers_creategrave { get; set; }
        public int settings_deadplayers_despawntime { get; set; }
        public string settings_default_gamemode { get; set; }
        public string settings_default_newplayer_group { get; set; }
        public int settings_delete_old_serverlogs_hours { get; set; }
        public int settings_illegal_state_limit { get; set; }
        public int settings_item_despawntime { get; set; }
        public int settings_max_npc { get; set; }
        public int settings_max_players { get; set; }
        public bool settings_monsters_enabled { get; set; }
        public double settings_npc_spawnrate { get; set; }
        public bool settings_peacefulmode_enabled { get; set; }
        public bool settings_pvp_enabled { get; set; }
        public bool settings_rcon_forward_commands { get; set; }
        public bool settings_rcon_forward_lua { get; set; }
        public bool settings_show_luaplugins { get; set; }
        public bool settings_show_playerplaytime { get; set; }
        public bool settings_show_restart_notification { get; set; }
        public int settings_spawnprotection_duration { get; set; }
        public string settings_start_weather { get; set; }
        public double settings_time_speed { get; set; }
        public string settings_weather_enabled { get; set; }
        public string settings_weather_preset { get; set; }
        public bool settings_whitelisted { get; set; }
        public string webadmin { get; set; }
    }
}
