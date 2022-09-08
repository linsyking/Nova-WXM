local poses = {
    ['xiben'] = {
        ['normal'] = {'body'},
    },
	['lanhu'] = {
        ['normal'] = {'tiger'},
        ['jishu'] = {'tiger','jishu_eye','jishu_tshirt'},
        ['zhuxi'] = {'tiger','zhuxi_cloth'},
        ['sheji'] = {'tiger','sheji_chocker','sheji_ear','sheji_panda'},
        ['wenan'] = {'tiger','wenan_hat','wenan_pen','wenan_penguin'}
    },
	['lanhu2'] = {
        ['normal'] = {'tiger'},
        ['jishu'] = {'tiger','jishu_eye','jishu_tshirt'},
        ['zhuxi'] = {'tiger','zhuxi_cloth'},
        ['sheji'] = {'tiger','sheji_chocker','sheji_ear','sheji_panda'},
        ['wenan'] = {'tiger','wenan_hat','wenan_pen','wenan_penguin'}
    }
}

function get_pose(obj, pose_name)
    return poses[obj.luaGlobalName] and poses[obj.luaGlobalName][pose_name]
end
