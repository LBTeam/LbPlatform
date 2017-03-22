-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               5.6.17 - MySQL Community Server (GPL)
-- Server OS:                    Win32
-- HeidiSQL version:             7.0.0.4218
-- Date/time:                    2017-03-22 13:48:38
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Dumping database structure for player
DROP DATABASE IF EXISTS `player`;
CREATE DATABASE IF NOT EXISTS `player` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `player`;


-- Dumping structure for table player.player_access
DROP TABLE IF EXISTS `player_access`;
CREATE TABLE IF NOT EXISTS `player_access` (
  `role_id` int(11) NOT NULL DEFAULT '0' COMMENT '角色ID，palyer_role表id外键',
  `node_id` int(11) NOT NULL DEFAULT '0' COMMENT '节点ID，palyer_node表id外键',
  `level` tinyint(1) NOT NULL DEFAULT '1',
  `module` varchar(64) DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='权限表';

-- Data exporting was unselected.


-- Dumping structure for table player.player_alarm
DROP TABLE IF EXISTS `player_alarm`;
CREATE TABLE IF NOT EXISTS `player_alarm` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `screen_id` int(11) NOT NULL DEFAULT '0' COMMENT '屏ID，player_screen表id外键',
  `type` tinyint(4) NOT NULL DEFAULT '0' COMMENT '监控数据类型，0-播放机参数信息',
  `param` text NOT NULL COMMENT '监控数据',
  `up_time` int(11) NOT NULL DEFAULT '0' COMMENT '监控数据上报时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='监控数据';

-- Data exporting was unselected.


-- Dumping structure for table player.player_alarm_set
DROP TABLE IF EXISTS `player_alarm_set`;
CREATE TABLE IF NOT EXISTS `player_alarm_set` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `screen_id` int(11) NOT NULL DEFAULT '0',
  `cpu_usage` int(10) NOT NULL DEFAULT '0' COMMENT 'CPU使用率',
  `disk_usage` int(10) NOT NULL DEFAULT '0' COMMENT '硬盘使用率',
  `memory_usage` int(10) NOT NULL DEFAULT '0' COMMENT '内存使用率',
  `cpu_temperature` int(10) NOT NULL DEFAULT '0' COMMENT 'CPU温度',
  `fan_speed` int(10) NOT NULL DEFAULT '0' COMMENT '风扇转速',
  `is_auto` tinyint(2) NOT NULL DEFAULT '0' COMMENT '自动告警，0-手动，1-自动',
  `alarm_mode` tinyint(2) NOT NULL DEFAULT '1' COMMENT '告警模式，0-手机短信，1-邮件',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='告警配置';

-- Data exporting was unselected.


-- Dumping structure for table player.player_command
DROP TABLE IF EXISTS `player_command`;
CREATE TABLE IF NOT EXISTS `player_command` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `screen_id` int(11) NOT NULL DEFAULT '0' COMMENT '屏ID，player_screen表id外键',
  `type` tinyint(4) NOT NULL DEFAULT '0' COMMENT '命令类型，0-发布播放方案，1-锁定屏幕参数更新，2-心跳周期更新，3-监控数据上传参数更新，4-软件定时开关时间更新',
  `param` text NOT NULL COMMENT '命令详情',
  `publish` int(11) NOT NULL DEFAULT '0' COMMENT '命令发布时间',
  `execute` int(11) NOT NULL DEFAULT '0' COMMENT '命令执行时间',
  `expired` int(11) NOT NULL DEFAULT '0' COMMENT '命令过期时间',
  `status` tinyint(4) NOT NULL DEFAULT '0' COMMENT '命令状态，0-已发布（未下发），1-已下发，2-执行成功，3-执行失败，4-命令作废',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='命令';

-- Data exporting was unselected.


-- Dumping structure for table player.player_config
DROP TABLE IF EXISTS `player_config`;
CREATE TABLE IF NOT EXISTS `player_config` (
  `key` varchar(64) NOT NULL DEFAULT '' COMMENT '键',
  `value` varchar(128) NOT NULL DEFAULT '' COMMENT '值'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='系统设置';

-- Data exporting was unselected.


-- Dumping structure for table player.player_group
DROP TABLE IF EXISTS `player_group`;
CREATE TABLE IF NOT EXISTS `player_group` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `uid` int(11) NOT NULL DEFAULT '0' COMMENT '用户ID，player_user表uid外键',
  `name` varchar(64) NOT NULL DEFAULT '' COMMENT '分组名称',
  `remark` varchar(255) NOT NULL DEFAULT '' COMMENT '分组描述',
  `addtime` int(11) NOT NULL DEFAULT '0' COMMENT '添加时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='屏幕组';

-- Data exporting was unselected.


-- Dumping structure for table player.player_group_screen
DROP TABLE IF EXISTS `player_group_screen`;
CREATE TABLE IF NOT EXISTS `player_group_screen` (
  `uid` int(11) NOT NULL DEFAULT '0' COMMENT '用户ID，player_user表uid外键',
  `screen_id` int(11) NOT NULL DEFAULT '0' COMMENT '屏ID，player_screen表id外键',
  `group_id` int(11) NOT NULL DEFAULT '0' COMMENT '屏幕组ID，player_group表id外键'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='屏幕分组及屏幕对应表';

-- Data exporting was unselected.


-- Dumping structure for table player.player_media
DROP TABLE IF EXISTS `player_media`;
CREATE TABLE IF NOT EXISTS `player_media` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL DEFAULT '0' COMMENT '用户ID，palyer_user表uid外键',
  `name` varchar(128) NOT NULL DEFAULT '' COMMENT '媒体名字',
  `md5` varchar(64) NOT NULL DEFAULT '' COMMENT '媒体文件md5',
  `object` varchar(128) NOT NULL DEFAULT '' COMMENT '媒体文件oss存储对象名称',
  `upload_id` varchar(64) NOT NULL DEFAULT '' COMMENT 'oss存储uploadId',
  `size` varchar(64) NOT NULL DEFAULT '' COMMENT '媒体文件大小',
  `status` tinyint(4) NOT NULL DEFAULT '0' COMMENT '上传状态，0-未上传完成，1-上传完成',
  `publish` int(11) NOT NULL DEFAULT '0' COMMENT '发布时间',
  `expired` int(11) NOT NULL DEFAULT '0' COMMENT '过期时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='播放媒体';

-- Data exporting was unselected.


-- Dumping structure for table player.player_node
DROP TABLE IF EXISTS `player_node`;
CREATE TABLE IF NOT EXISTS `player_node` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(64) NOT NULL DEFAULT '' COMMENT '节点英文标识',
  `title` varchar(64) NOT NULL DEFAULT '' COMMENT '节点名称',
  `status` tinyint(2) NOT NULL DEFAULT '0' COMMENT '状态；0开启，1关闭',
  `remark` varchar(255) DEFAULT '' COMMENT '备注',
  `sort` smallint(6) DEFAULT '1' COMMENT '排序',
  `pid` int(11) NOT NULL DEFAULT '1' COMMENT '父ID',
  `level` tinyint(2) NOT NULL DEFAULT '1' COMMENT '级别（类型）；1模块，2列表，3操作',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='节点表';

-- Data exporting was unselected.


-- Dumping structure for table player.player_player
DROP TABLE IF EXISTS `player_player`;
CREATE TABLE IF NOT EXISTS `player_player` (
  `id` int(11) NOT NULL DEFAULT '0' COMMENT '屏ID，和player_screen表id一一对应',
  `bind_id` varchar(64) NOT NULL DEFAULT '' COMMENT '屏绑定ID，播放器绑定使用',
  `bind_key` varchar(64) NOT NULL DEFAULT '' COMMENT '屏绑定Key，播放器绑定使用',
  `name` varchar(64) NOT NULL DEFAULT '' COMMENT '播放器名称',
  `remark` varchar(255) NOT NULL DEFAULT '' COMMENT '播放器描述',
  `mode` varchar(64) NOT NULL DEFAULT '' COMMENT '播放器模式',
  `start` varchar(32) NOT NULL DEFAULT '' COMMENT '工作开始时间',
  `end` varchar(32) NOT NULL DEFAULT '' COMMENT '工作结束时间',
  `mac` varchar(20) DEFAULT '' COMMENT '播放器MAC地址',
  `version` varchar(64) DEFAULT '' COMMENT '播放端版本号',
  `heartbeat_interval` smallint(6) DEFAULT '0' COMMENT '心跳间隔（秒）',
  `next_heartbeat` int(11) DEFAULT '0' COMMENT '下次心跳时间'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='播放器';

-- Data exporting was unselected.


-- Dumping structure for table player.player_player_setting
DROP TABLE IF EXISTS `player_player_setting`;
CREATE TABLE IF NOT EXISTS `player_player_setting` (
  `id` int(11) NOT NULL DEFAULT '0' COMMENT '屏ID，和player_screen表id对应',
  `clock` tinyint(2) NOT NULL DEFAULT '0' COMMENT '锁定屏幕；0-锁定，1-开启',
  `clock_password` varchar(16) NOT NULL DEFAULT '' COMMENT '锁定密码',
  `heartbeat_cycle` varchar(16) NOT NULL DEFAULT '' COMMENT '心跳周期；单位：秒（s）',
  `alarm_cycle` varchar(16) NOT NULL DEFAULT '' COMMENT '监控数据上报周期 ；单位：秒（s）',
  `alarm_url` varchar(255) NOT NULL DEFAULT '' COMMENT '监控数据上报路径',
  `time_switch` tinyint(2) NOT NULL DEFAULT '0' COMMENT '定时开关；0-禁用，1-启用',
  `soft_enable` varchar(16) NOT NULL DEFAULT '' COMMENT '定时开启时间',
  `soft_disable` varchar(16) NOT NULL DEFAULT '' COMMENT '定时关闭时间'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='播放器配置参数';

-- Data exporting was unselected.


-- Dumping structure for table player.player_price
DROP TABLE IF EXISTS `player_price`;
CREATE TABLE IF NOT EXISTS `player_price` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `screen_id` int(11) NOT NULL DEFAULT '0' COMMENT '屏ID，palyer_screen表id外键',
  `start` varchar(32) NOT NULL DEFAULT '' COMMENT '开始时间',
  `end` varchar(32) NOT NULL DEFAULT '' COMMENT '结束时间',
  `price` decimal(10,2) NOT NULL DEFAULT '0.00' COMMENT '价格',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='时段价格表';

-- Data exporting was unselected.


-- Dumping structure for table player.player_program
DROP TABLE IF EXISTS `player_program`;
CREATE TABLE IF NOT EXISTS `player_program` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL DEFAULT '0' COMMENT 'palyer_user表uid外键',
  `name` varchar(128) NOT NULL DEFAULT '' COMMENT '播放方案名称',
  `object` varchar(128) NOT NULL DEFAULT '' COMMENT '播放方案oss存储对象名称',
  `upload_id` varchar(64) NOT NULL DEFAULT '' COMMENT 'oss存储uploadId',
  `info` text NOT NULL,
  `md5` varchar(64) NOT NULL DEFAULT '' COMMENT '播放方案文件md5',
  `type` tinyint(4) NOT NULL DEFAULT '0' COMMENT '播放方案类型，0-普通方案，7-紧急插播，8-离线方案',
  `size` varchar(20) NOT NULL DEFAULT '',
  `status` tinyint(4) NOT NULL DEFAULT '0' COMMENT '上传状态，0-未上传完成，1-上传完成',
  `backup` tinyint(4) NOT NULL DEFAULT '0' COMMENT '备份状态，0-未备份，1-已备份',
  `publish` int(11) NOT NULL DEFAULT '0' COMMENT '发布时间',
  `expired` int(11) NOT NULL DEFAULT '0' COMMENT '过期时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='播放方案';

-- Data exporting was unselected.


-- Dumping structure for table player.player_program_media
DROP TABLE IF EXISTS `player_program_media`;
CREATE TABLE IF NOT EXISTS `player_program_media` (
  `program_id` int(11) NOT NULL DEFAULT '0' COMMENT '播放方案ID',
  `media_id` int(11) NOT NULL DEFAULT '0' COMMENT '媒体ID'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='播放方案与媒体对应表';

-- Data exporting was unselected.


-- Dumping structure for table player.player_record
DROP TABLE IF EXISTS `player_record`;
CREATE TABLE IF NOT EXISTS `player_record` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `screen_id` int(11) NOT NULL DEFAULT '0' COMMENT '屏幕ID',
  `media_name` varchar(128) NOT NULL DEFAULT '' COMMENT '媒体文件名称',
  `media_md5` varchar(64) NOT NULL DEFAULT '' COMMENT '媒体文件MD5',
  `start` datetime NOT NULL DEFAULT '0000-00-00 00:00:00' COMMENT '播放开始时间',
  `end` datetime NOT NULL DEFAULT '0000-00-00 00:00:00' COMMENT '播放结束时间',
  `addtime` int(11) NOT NULL DEFAULT '0' COMMENT '上报时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='播放记录';

-- Data exporting was unselected.


-- Dumping structure for table player.player_region
DROP TABLE IF EXISTS `player_region`;
CREATE TABLE IF NOT EXISTS `player_region` (
  `id` smallint(5) unsigned NOT NULL AUTO_INCREMENT,
  `parent_id` smallint(5) unsigned NOT NULL DEFAULT '0',
  `name` varchar(120) NOT NULL DEFAULT '',
  `type` tinyint(1) NOT NULL DEFAULT '2',
  `agency_id` smallint(5) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Data exporting was unselected.


-- Dumping structure for table player.player_role
DROP TABLE IF EXISTS `player_role`;
CREATE TABLE IF NOT EXISTS `player_role` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(64) NOT NULL DEFAULT '' COMMENT '角色名称',
  `pid` smallint(6) NOT NULL DEFAULT '0',
  `status` tinyint(2) NOT NULL DEFAULT '0' COMMENT '状态；0开启，1关闭',
  `remark` varchar(255) DEFAULT '' COMMENT '备注',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='角色表';

-- Data exporting was unselected.


-- Dumping structure for table player.player_role_user
DROP TABLE IF EXISTS `player_role_user`;
CREATE TABLE IF NOT EXISTS `player_role_user` (
  `role_id` int(11) NOT NULL DEFAULT '0' COMMENT '角色ID，player_role表id外键',
  `user_id` int(11) NOT NULL DEFAULT '0' COMMENT '用户ID，player_user表uid外键'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='用户角色对应表';

-- Data exporting was unselected.


-- Dumping structure for table player.player_screen
DROP TABLE IF EXISTS `player_screen`;
CREATE TABLE IF NOT EXISTS `player_screen` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(64) NOT NULL DEFAULT '' COMMENT '屏幕名称',
  `remark` varchar(255) NOT NULL DEFAULT '' COMMENT '屏幕描述',
  `size_x` int(10) NOT NULL DEFAULT '0' COMMENT '屏幕尺寸X',
  `size_y` int(10) NOT NULL DEFAULT '0' COMMENT '屏幕尺寸Y',
  `resolu_x` int(10) NOT NULL DEFAULT '0' COMMENT '分辨率X',
  `resolu_y` int(10) NOT NULL DEFAULT '0' COMMENT '分辨率Y',
  `type` tinyint(2) NOT NULL DEFAULT '0' COMMENT '屏幕类型，0-室外，1-室内',
  `operate` tinyint(2) NOT NULL DEFAULT '0' COMMENT '运作方式，0-全包，1-分时',
  `longitude` varchar(64) NOT NULL DEFAULT '' COMMENT '经度',
  `latitude` varchar(64) NOT NULL DEFAULT '' COMMENT '纬度',
  `uid` int(11) NOT NULL DEFAULT '0' COMMENT '拥有者，player_user表uid外键',
  `province` int(11) NOT NULL DEFAULT '0' COMMENT '省',
  `city` int(11) NOT NULL DEFAULT '0' COMMENT '市',
  `district` int(11) DEFAULT '0' COMMENT '区',
  `address` varchar(255) NOT NULL DEFAULT '' COMMENT '街道',
  `file` varchar(255) DEFAULT '' COMMENT '文件附件',
  `db_version` varchar(255) DEFAULT '' COMMENT '数据库版本',
  `is_delete` tinyint(2) DEFAULT '0' COMMENT '是否删除，0-未删除，1-已删除',
  `addtime` int(11) NOT NULL DEFAULT '0' COMMENT '添加时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='屏幕表';

-- Data exporting was unselected.


-- Dumping structure for table player.player_user
DROP TABLE IF EXISTS `player_user`;
CREATE TABLE IF NOT EXISTS `player_user` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(64) DEFAULT '' COMMENT '用户名',
  `password` varchar(64) NOT NULL DEFAULT '' COMMENT '密码',
  `email` varchar(128) NOT NULL DEFAULT '' COMMENT '邮箱',
  `phone` varchar(16) DEFAULT '' COMMENT '手机',
  `realname` varchar(64) DEFAULT '' COMMENT '真实姓名',
  `address` varchar(128) DEFAULT '' COMMENT '地址',
  `puid` int(11) NOT NULL DEFAULT '0' COMMENT '父UID',
  `status` tinyint(2) NOT NULL DEFAULT '0' COMMENT '用户状态，0-正常，1-禁用，2-未认证',
  `type` tinyint(2) NOT NULL DEFAULT '0' COMMENT '用户类型，0-管理员，1-代理用户，2-普通用户',
  `is_del` tinyint(2) NOT NULL DEFAULT '0' COMMENT '是否删除，0-未删除，1-已删除',
  `is_full` tinyint(2) NOT NULL DEFAULT '0' COMMENT '是否完善，0-已完善，1-未完善',
  `lasttime` int(11) DEFAULT '0' COMMENT '上次登录时间',
  `lastip` varchar(16) DEFAULT '' COMMENT '上次登录地址',
  `addtime` int(11) NOT NULL DEFAULT '0' COMMENT '添加时间',
  `reg_code` varchar(32) DEFAULT '' COMMENT '注册码，代理用户使用',
  `token` varchar(64) DEFAULT '' COMMENT '登录令牌',
  `expire` int(11) DEFAULT '0' COMMENT '令牌过期时间',
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='用户表';

-- Data exporting was unselected.


-- Dumping structure for table player.player_version
DROP TABLE IF EXISTS `player_version`;
CREATE TABLE IF NOT EXISTS `player_version` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL DEFAULT '' COMMENT '版本名',
  `url` varchar(255) NOT NULL DEFAULT '' COMMENT '存储地址',
  `version` varchar(255) NOT NULL DEFAULT '' COMMENT '版本号',
  `addtime` int(11) NOT NULL DEFAULT '0' COMMENT '发布时间',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='版本列表';

-- Data exporting was unselected.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
