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

float getSample(vec2 t)
{
	float s = texture2D(spectrumTex,t).r;

	//s = 40.0*log(s+1.0);

	//s = 4.0 * sqrt(s);

	s = 20.0*log(s);
	s = max(0.0,1.0 + ((s+20.0) / 160.0));

	//s = 5.0 * pow(s,1.0/sqrt(2.0));

	return s;
}

vec2 texel = vec2(1.0/2048.0,0.0);

vec4 renderSpectrum(vec2 t)
{
	t.x = t.x * 0.1 + 0.9 * t.x * t.x;

	float s = getSample(t);

	vec4 col = vec4(0.0,0.0,0.0,1.0);
	vec4 col0 = vec4(0.0,0.0,0.0,1.0);
	vec4 col1 = vec4(0.04,0.02,0.7,1.0);
	vec4 col2 = vec4(0.1,0.7,0.8,1.0);
	vec4 col3 = vec4(0.9,0.8,0.05,1.0);
	vec4 col4 = vec4(0.9,0.1,0.02,1.0);
	vec4 col5 = vec4(0.95,0.95,0.95,1.0);
	
	col = mix(col0,col1,clamp(s*10.0,0.0,1.0));
	col = mix(col,col2,clamp((s-0.125)*3.0,0.0,1.0));
	col = mix(col,col3,clamp((s-0.25)*3.0,0.0,1.0));
	col = mix(col,col4,clamp((s-0.5)*4.0,0.0,1.0));
	col = mix(col,col5,clamp((s-0.75)*8.0,0.0,1.0));

	return col;
}

vec4 renderGraph(vec2 t)
{
	vec4 col = vec4(0.0,0.0,0.0,1.0);

	float s = getSample(vec2(t.y,0.0));

	if (s > t.x)
		col = vec4(1.0);

	return col;
}


void main(void)
{
	//texcoord.y *= texcoord.y;
	vec2 t = texcoord.yx;

	vec4 col;

	if (t.x>0.1)
	{
		vec2 t2 = vec2((t.x-0.1)/0.9,t.y);
		col = renderSpectrum(t2);
	}
	else
	{
		col = renderGraph(vec2(t.x*10.0,t.y));
	}

	out_Colour = col;	
}

