# NDMF VRCF Reorderer

# <span style="color:red">Installing this package will make your VRCFury install unsupported, if you encounter any bugs try uninstalling this, restarting unity and checking if your still run into it.</span>
# <span style="color:red">The original functionality provided by this package has been incorporated into VRCFury version 1.648.0 onwards and NDMF version 1.3.0 onwards. It is still usable for running vrcfury at a different time in NDMF's build process.</span>

[VCC Repo](https://bigibas123.github.io/VCC/)

Tries to make running [VRCFury](https://vrcfury.com/) with [NDMF](https://github.com/bdunderscore/ndmf.git) and [AvatarOptimizer](https://github.com/anatawa12/AvatarOptimizer.git) a bit more predictable.

It does this by removing the main vrcfury from the avatar building hooks and calling VRCFury at the end of the optimization step of NDMF instead.
This is done with reflection so I don't expect it to remain stable for longer periods of time.

If you have any sugestions about how to do it better I'm all ears or submit a pull request if you're able to.


## Background

I wrote this because VRCFury creates a seperate material for each skinned mesh, which makes AvatarOptimizers's mesh join tool unable to merge the materials. Reversing the VRCFury AvatarOptimizer order allows AvatarOptimizer to do it's thing and then have VRCFury clobber the avatar. 


