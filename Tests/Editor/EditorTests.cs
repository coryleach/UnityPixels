using NUnit.Framework;

namespace Gameframe.Pixels.Editor.Tests
{
    public class EditorTests
    {
        [Test]
        public void SpriteFaceCheck()
        {
            var spriteFaces = SpriteFaces.Front;
            Assert.IsTrue(spriteFaces.Check(SpriteFaces.Front));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Back));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Top));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Bottom));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Left));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Right));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.All));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.None));

            spriteFaces = SpriteFaces.All;
            Assert.IsTrue(spriteFaces.Check(SpriteFaces.Front));
            Assert.IsTrue(spriteFaces.Check(SpriteFaces.Back));
            Assert.IsTrue(spriteFaces.Check(SpriteFaces.Left));
            Assert.IsTrue(spriteFaces.Check(SpriteFaces.Right));
            Assert.IsTrue(spriteFaces.Check(SpriteFaces.Top));
            Assert.IsTrue(spriteFaces.Check(SpriteFaces.Bottom));
            Assert.IsTrue(spriteFaces.Check(SpriteFaces.All));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.None));
            
            spriteFaces = SpriteFaces.None;
            Assert.IsTrue(spriteFaces.Check(SpriteFaces.None));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Front));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Back));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Left));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Right));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Top));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.Bottom));
            Assert.IsFalse(spriteFaces.Check(SpriteFaces.All));
        }
    } 
}


