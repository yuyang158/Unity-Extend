import bpy
import sys
import os

argv = sys.argv
argv = argv[argv.index("--") + 1:]  # get all args after "--"

ratio = float(argv[0]) / 100
print(ratio)
fbxFile = argv[1]
print(fbxFile)

bpy.ops.import_scene.fbx(filepath=fbxFile, use_anim=False)

bpy.ops.object.select_all(action='DESELECT')
bpy.data.objects['Cube'].select_set(True)
bpy.ops.object.delete()

bpy.ops.object.select_by_type(type='MESH')

decimateIteration = 8
print('*' * 50)
objects = bpy.context.selected_objects
for mesh in objects:
    bpy.context.view_layer.objects.active = mesh
    print(bpy.context.active_object)
    bpy.ops.object.modifier_add(type='DECIMATE')
    bpy.context.object.modifiers["Decimate"].iterations = decimateIteration
    bpy.context.object.modifiers["Decimate"].ratio = ratio

print('*' * 50)

exportPathParts = os.path.splitext(fbxFile)
bpy.ops.export_scene.fbx(filepath=exportPathParts[0] + '-export' + exportPathParts[1], object_types={"MESH"})