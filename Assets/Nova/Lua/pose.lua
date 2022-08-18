local poses = {
    ['xiben'] = {
        ['normal'] = {'body'},
    },
}

function get_pose(obj, pose_name)
    return poses[obj.luaGlobalName] and poses[obj.luaGlobalName][pose_name]
end
