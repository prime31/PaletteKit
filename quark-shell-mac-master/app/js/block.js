
var BACKGROUND_COLOR = tinycolor( 'white' );

function mathSign( num )
{
	return num ? num < 0 ? -1 : 1 : 0;
}

function Block( element, i, j )
{
    this.i = i;
    this.j = j;
    this.element = element;
    this.clearColor();
}

Block.prototype.getColor = function()
{
    return this.color;
};

Block.prototype.setColor = function( color )
{
    this.color = tinycolor( color );
    this.element.css( 'background-color', this.color.toHexString() );
    this.element.css( 'border-color', tinycolor.mostReadable( this.color, this.color.tetrad(), { includeFallbackColors: true } ).toHexString() );
};

Block.prototype.hasColor = function()
{
    return !tinycolor.equals( this.color, BACKGROUND_COLOR );
};

Block.prototype.clearColor = function()
{
    this.setColor( BACKGROUND_COLOR );
};





function BlockModel( root, width, height )
{
    this.root = $( root );
    this.width = width;
    this.height = height;
    this.blocks = new Array( width );
    
    // fill our blocks
    for( var i = 0; i < this.blocks.length; i++ )
        this.blocks[i] = new Array( height );
    
    for( var i = 0; i < width; i++ )
    {
        for( var j = 0; j < height; j++ )
        {
            var ele = $( '<div/>', { class: 'block' } );
            ele.css( 'width', ( 1 / (width + 1) ) * 100 + '%' );
            ele.css( 'height', ( 1 / (height + 1) ) * 100 + '%' );
            this.root.prepend( ele );
            
            var x = width - 1 - j;
            this.blocks[x][i] = new Block( ele, x, i );
            ele.data( { 'block': this.blocks[x][i] } );
        }
    }
};

BlockModel.prototype.centerBlock = function()
{
    var halfWidth = Math.ceil( this.width / 2 );
    var halfHeight = Math.ceil( this.height / 2 );
    return this.blocks[halfWidth][halfHeight];
};

BlockModel.prototype.clear = function()
{
    for( var i = 0; i < width; i++ )
    {
        for( var j = 0; j < height; j++ )
        {
            this.blocks[i][j].clearColor();
        }
    }
};

BlockModel.prototype.indexOf = function( block )
{
    for( var i = 0; i < width; i++ )
    {
        for( var j = 0; j < height; j++ )
        {
            if( this.blocks[i][j] == block )
                return { x: i, y: j };
        }
    }
};

BlockModel.prototype.tryFillVertical = function( block, y )
{
    if( !block.hasColor() )
        return;
        
    var x = block.i;
    var newY = block.j + y;
    var sign = mathSign( y );
    var steps = 0;
    
    while( newY >= 0 && newY < this.height && !this.blocks[x][newY].hasColor() )
    {
        steps++;
        newY += y;
    }
    
    if( newY >= 0 && newY < this.height && steps > 0 )
    {
        var startColor = block.getColor();
        var endColor = this.blocks[x][newY].getColor();
        var stepOffset = steps + 1;
        
        for( var i = 1; i <= steps; i++ )
        {
            var b = this.blocks[x][block.j + i * sign];
            b.setColor( tinycolor.mix( startColor, endColor, 100 * i / stepOffset ) );
        }
    }
	else
	{
		$( '#negativeSound' )[0].play()
	}
};

BlockModel.prototype.tryShadow = function( block )
{
    var newX = block.i - 1;
    if( newX >= 0 && newX < this.width )
    {
        var b = this.blocks[newX][block.j];
        var hsl = block.getColor().toHsl();
        //hsl.h = hsl.h + mathSign( hsl.h - shadowMagnetColor.toHsv().h ) * 14;
        hsl.h = hsl.h + mathSign( shadowMagnetColor.toHsv().h - hsl.h ) * 14;
        hsl.s = ( hsl.s * 100 );
        hsl.l = ( hsl.l * 100 ) - 14;
        b.setColor( hsl );
        
        return b;
    }
    
    return block;
};

BlockModel.prototype.tryHighlight = function( block )
{
    var newX = block.i + 1;
    
    if( newX >= 0 && newX < this.width )
    {
        var b = this.blocks[newX][block.j];
        var hsl = block.getColor().toHsl();
        hsl.h = hsl.h + mathSign( highlightMagnetColor.toHsl().h - hsl.h ) * 14;
        hsl.s = ( hsl.s * 100 ) - 0;
        hsl.l = ( hsl.l * 100 ) + 14;
        b.setColor( hsl );
        
        return b;
    }
    
    return block;
};

BlockModel.prototype.setBackgroundColor = function( color )
{
    var oldColor = BACKGROUND_COLOR;
    BACKGROUND_COLOR = tinycolor( color );
    
    for( var i = 0; i < width; i++ )
    {
        for( var j = 0; j < height; j++ )
        {
            var b = this.blocks[i][j];
            if( tinycolor.equals( b.getColor(), oldColor ) )
                b.setColor( BACKGROUND_COLOR );
        }
    }
};

BlockModel.prototype.addColorsNearBlock = function( block, colors )
{
	var blockColor = block.getColor();
	if( tinycolor.equals( blockColor, colors[0] ) )
		colors.shift();
	
    var colorsAdded = 0;
    
    // add as many as we can right, then left
    var x = block.i + 1;
    var y = block.j;
    while( colors.length > 0 && x < this.width && !this.blocks[x][y].hasColor() )
    {
        this.blocks[x][y].setColor( colors.shift() );
        x += 1;
    }
    
    x = block.i - 1;
    while( colors.length > 0 && x >= 0 && !this.blocks[x][y].hasColor() )
    {
        this.blocks[x][y].setColor( colors.shift() );
        x -= 1;
    }
    
    x = block.i;
    y = block.j + 1;
    while( colors.length > 0 && y < this.height && !this.blocks[x][y].hasColor() )
    {
        this.blocks[x][y].setColor( colors.shift() );
        y += 1;
    }
    
    x = block.i;
    y = block.j - 1;
    while( colors.length > 0 && y >= 0 && !this.blocks[x][y].hasColor() )
    {
        this.blocks[x][y].setColor( colors.shift() );
        y -= 1;
    }
    
    if( colors.length > 0 )
	{
		$( '#negativeSound' )[0].play()
        alert( 'Ran out of room adding the colors around the selected color. There were ' + colors.length + ' colors that were not added.' );
	}
};

BlockModel.prototype.tryAddColorsInLine = function( block, colors )
{
    var y = 1;
    var x = block.i;
    var newY = block.j + y;
    var steps = 0;
    
    while( newY >= 0 && newY < this.height && !this.blocks[x][newY].hasColor() )
    {
        steps++;
        newY += y;
    }
};

BlockModel.prototype.getColorsInJascFormat = function()
{
	var colors = [];
    for( var i = 0; i < width; i++ )
    {
        for( var j = 0; j < height; j++ )
        {
            var b = this.blocks[i][j];
            if( b.hasColor() )
            {
				var rgb = b.getColor().toRgb();
				colors.push( rgb.r + ' ' + rgb.g + ' ' + rgb.b );
			}
        }
    }
	
	return colors;
};
