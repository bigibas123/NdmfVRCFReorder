# NDMF VRCF Reorderer
# <span style="color:red">Should not be necessary anymore from VRCFury 1.648.0 onwards</span>

Tries to make running [VRCFury](https://vrcfury.com/) with [NDMF](https://github.com/bdunderscore/ndmf.git) and [AvatarOptimizer](https://github.com/anatawa12/AvatarOptimizer.git) a bit more predictable.

It does this by removing vrcfury from the avatar building hooks and calling VRCFury at the end of the optimization step of NDMF instead.
This is done with reflection so I don't expect it to remain stable for longer periods of time.


I wrote this because VRCFury creates a seperate material for each skinned mesh, which makes AvatarOptimizers's mesh join tool unable to merge the materials. Reversing the VRCFury AvatarOptimizer order allows AvatarOptimizer to do it's thing and then have VRCFury clobber the avatar.

If you have any sugestions about how to do it better I'm all ears or submit a pull request if you're able to.

