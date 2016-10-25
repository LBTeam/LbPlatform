-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               5.6.17 - MySQL Community Server (GPL)
-- Server OS:                    Win32
-- HeidiSQL version:             7.0.0.4218
-- Date/time:                    2016-10-25 18:16:44
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Dumping database structure for player
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

-- Dumping data for table player.player_access: ~76 rows (approximately)
/*!40000 ALTER TABLE `player_access` DISABLE KEYS */;
INSERT INTO `player_access` (`role_id`, `node_id`, `level`, `module`) VALUES
	(1, 1, 1, ''),
	(1, 13, 1, ''),
	(1, 24, 1, ''),
	(1, 25, 1, ''),
	(1, 26, 1, ''),
	(1, 27, 1, ''),
	(1, 3, 1, ''),
	(1, 4, 1, ''),
	(1, 34, 1, ''),
	(1, 35, 1, ''),
	(1, 36, 1, ''),
	(1, 51, 1, ''),
	(1, 58, 1, ''),
	(1, 59, 1, ''),
	(1, 10, 1, ''),
	(1, 11, 1, ''),
	(1, 41, 1, ''),
	(1, 18, 1, ''),
	(1, 19, 1, ''),
	(1, 42, 1, ''),
	(1, 43, 1, ''),
	(1, 44, 1, ''),
	(1, 45, 1, ''),
	(1, 46, 1, ''),
	(1, 47, 1, ''),
	(1, 48, 1, ''),
	(1, 49, 1, ''),
	(1, 52, 1, ''),
	(1, 54, 1, ''),
	(1, 53, 1, ''),
	(1, 56, 1, ''),
	(1, 57, 1, ''),
	(2, 1, 1, ''),
	(2, 2, 1, ''),
	(2, 20, 1, ''),
	(2, 21, 1, ''),
	(2, 22, 1, ''),
	(2, 23, 1, ''),
	(2, 3, 1, ''),
	(2, 4, 1, ''),
	(2, 34, 1, ''),
	(2, 35, 1, ''),
	(2, 36, 1, ''),
	(2, 51, 1, ''),
	(2, 58, 1, ''),
	(2, 59, 1, ''),
	(2, 18, 1, ''),
	(2, 19, 1, ''),
	(2, 42, 1, ''),
	(2, 55, 1, ''),
	(2, 43, 1, ''),
	(2, 44, 1, ''),
	(2, 45, 1, ''),
	(2, 46, 1, ''),
	(2, 47, 1, ''),
	(2, 48, 1, ''),
	(2, 49, 1, ''),
	(2, 52, 1, ''),
	(2, 54, 1, ''),
	(2, 53, 1, ''),
	(2, 56, 1, ''),
	(2, 57, 1, ''),
	(2, 60, 1, ''),
	(2, 61, 1, ''),
	(2, 62, 1, ''),
	(3, 3, 1, ''),
	(3, 4, 1, ''),
	(3, 34, 1, ''),
	(3, 35, 1, ''),
	(3, 36, 1, ''),
	(3, 51, 1, ''),
	(3, 18, 1, ''),
	(3, 19, 1, ''),
	(3, 55, 1, ''),
	(3, 43, 1, ''),
	(3, 46, 1, '');
/*!40000 ALTER TABLE `player_access` ENABLE KEYS */;


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
) ENGINE=InnoDB AUTO_INCREMENT=63 DEFAULT CHARSET=utf8 COMMENT='节点表';

-- Dumping data for table player.player_node: ~61 rows (approximately)
/*!40000 ALTER TABLE `player_node` DISABLE KEYS */;
INSERT INTO `player_node` (`id`, `name`, `title`, `status`, `remark`, `sort`, `pid`, `level`) VALUES
	(1, 'User', '用户管理', 0, '', 1, 0, 1),
	(2, 'index', '用户列表', 0, '', 1, 1, 2),
	(3, 'Group', '分组管理', 0, '', 3, 0, 1),
	(4, 'index', '分组列表', 0, '', 1, 3, 2),
	(5, 'Node', '节点管理', 0, '', 4, 0, 1),
	(6, 'index', '节点列表', 0, '', 1, 5, 2),
	(7, 'add', '添加节点', 0, '', 1, 6, 3),
	(8, 'Role', '用户组管理', 0, '', 2, 0, 1),
	(9, 'index', '用户组列表', 0, '', 1, 8, 2),
	(10, 'System', '系统管理', 0, '系统管理', 5, 0, 1),
	(11, 'index', '系统配置', 0, '系统配置', 1, 10, 2),
	(12, 'root_list', '管理员列表', 0, '', 2, 1, 2),
	(13, 'agent_list', '代理商列表', 0, '', 3, 1, 2),
	(14, 'add_root', '新增', 0, '', 1, 12, 3),
	(15, 'edit_root', '修改', 0, '', 2, 12, 3),
	(16, 'root_status', '启用 / 禁用', 0, '', 3, 12, 3),
	(17, 'del_root', '删除', 0, '', 4, 12, 3),
	(18, 'Screen', '屏幕管理', 0, '', 6, 0, 1),
	(19, 'index', '屏幕列表', 0, '', 1, 18, 2),
	(20, 'add', '添加用户', 0, '', 1, 2, 3),
	(21, 'edit', '修改用户', 0, '', 2, 2, 3),
	(22, 'del', '删除用户', 0, '', 3, 2, 3),
	(23, 'status', '启用 / 禁用', 0, '', 4, 2, 3),
	(24, 'add_agent', '新增', 0, '', 1, 13, 3),
	(25, 'edit_agent', '修改', 0, '', 2, 13, 3),
	(26, 'agent_status', '启用 / 禁用', 0, '', 3, 13, 3),
	(27, 'del_agent', '删除', 0, '', 4, 13, 3),
	(28, 'add', '添加', 0, '', 1, 9, 3),
	(29, 'edit', '修改', 0, '', 2, 9, 3),
	(30, 'del', '删除', 0, '', 3, 9, 3),
	(31, 'status', '启用 / 禁用', 0, '', 4, 9, 3),
	(32, 'user', '用户列表', 0, '', 5, 9, 3),
	(33, 'access', '授权', 0, '', 6, 9, 3),
	(34, 'add', '添加', 0, '', 1, 4, 3),
	(35, 'edit', '修改', 0, '', 2, 4, 3),
	(36, 'del', '删除', 0, '', 3, 4, 3),
	(37, 'edit', '修改节点', 0, '', 2, 6, 3),
	(38, 'del', '删除节点', 0, '', 3, 6, 3),
	(39, 'status', '启用 / 禁用', 0, '', 4, 6, 3),
	(40, 'sort', '排序', 0, '', 5, 6, 3),
	(41, 'save', '系统设置配置', 0, '', 1, 11, 3),
	(42, 'add', '添加', 0, '', 1, 19, 3),
	(43, 'edit', '修改', 0, '', 2, 19, 3),
	(44, 'del', '删除', 0, '', 3, 19, 3),
	(45, 'player', '播放器', 0, '', 4, 19, 3),
	(46, 'price', '时段价格', 0, '', 5, 19, 3),
	(47, 'add_price', '添加时段价格', 0, '', 6, 19, 3),
	(48, 'edit_price', '修改时段价格', 0, '', 7, 19, 3),
	(49, 'del_price', '删除时段价格', 0, '', 8, 19, 3),
	(51, 'screens', '屏幕列表', 0, '', 4, 4, 3),
	(52, 'setting', '播放器参数配置', 0, '', 9, 19, 3),
	(53, 'notify', '紧急通知', 0, '', 11, 19, 3),
	(54, 'shutdown', '一键关屏', 0, '', 10, 19, 3),
	(55, 'show', '查看', 0, '', 1, 19, 3),
	(56, 'monitor', '监控数据', 0, '', 12, 19, 3),
	(57, 'picture', '监控图片', 0, '', 13, 19, 3),
	(58, 'send_sms', '发送短信', 0, '', 5, 4, 3),
	(59, 'send_email', '发送邮件', 0, '', 6, 4, 3),
	(60, 'Record', '播放记录', 0, '', 7, 0, 1),
	(61, 'index', '记录列表', 0, '', 1, 60, 2),
	(62, 'export', '导出', 0, '', 1, 61, 3);
/*!40000 ALTER TABLE `player_node` ENABLE KEYS */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
