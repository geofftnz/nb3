//|vert
#version 410
precision highp float;
layout (location = 0) in vec3 vertex;
layout (location = 0) out vec2 texcoord;

void main() {

	gl_Position = vec4(vertex.xy,0.0,1.0);
	texcoord = vertex.xy * 0.5 + 0.5;
}



//|frag
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D spectrumTex;

void main(void)
{
	float s = texture2D(spectrumTex,texcoord).r;
	vec4 col = vec4(s,0.0,0.0,1.0);

	out_Colour = col;	
}

