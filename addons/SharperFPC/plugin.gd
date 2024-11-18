@tool
extends EditorPlugin

func _enter_tree():
	add_custom_type("SharperFirstPersonController", "CharacterBody3D", preload("res://addons/SharperFPC/CameraFirstPerson.cs"), preload("icon.svg"))


func _exit_tree():
	remove_custom_type("SharperFirstPersonController")
