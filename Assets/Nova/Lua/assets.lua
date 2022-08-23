function wxpreload(filename)
	__Nova.assetLoader:preloadAsset(filename)
end

function m_release_all()
	__Nova.assetLoader:m_release_all()
end

function m_release_all_without_locked()
	__Nova.assetLoader:m_release_all_without_locked()
end

function m_hold(dd)
	__Nova.assetLoader:m_hold(dd)
end

function m_unhold(dd)
	__Nova.assetLoader:m_unhold(dd)
end

function m_release(ff)
	__Nova.assetLoader:m_release(ff)
end