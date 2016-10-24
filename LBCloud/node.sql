-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               5.6.17 - MySQL Community Server (GPL)
-- Server OS:                    Win32
-- HeidiSQL version:             7.0.0.4218
-- Date/time:                    2016-10-23 16:02:13
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

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
) ENGINE=InnoDB AUTO_INCREMENT=58 DEFAULT CHARSET=utf8 COMMENT='节点表';

-- Dumping data for table player.player_node: ~53 rows (approximately)
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
	(57, 'picture', '监控图片', 0, '', 13, 19, 3);
/*!40000 ALTER TABLE `player_node` ENABLE KEYS */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
