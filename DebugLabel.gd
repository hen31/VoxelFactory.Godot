extends Label


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if(Input.is_key_pressed(KEY_P)):
		visible = !visible;
	if(visible):
		text = 'fps:'+ str(Performance.get_monitor(Performance.TIME_FPS)) + ' vertexes:'+str(Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME)) + ' draw calls:'+str(Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME)); 
	pass
