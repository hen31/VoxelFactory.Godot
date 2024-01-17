shader_type spatial;
render_mode unshaded;

uniform sampler2D AlbedoTexture : source_color;
uniform float AmbientLightingStrenght;
varying float light_strenght;

void vertex()
{
	light_strenght = CUSTOM0.a;
}
//light_strenght
void fragment() {
	ALBEDO = texture(AlbedoTexture, UV).rgb * ((vec3(1,1,1) * AmbientLightingStrenght) + (vec3(1,1,1) * light_strenght));
	//EMISSION = ALBEDO * ((vec3(1,1,1) * AmbientLightingStrenght));// + (vec3(1,1,1) * light_strenght));
	//ALPHA = 1.0;
}
